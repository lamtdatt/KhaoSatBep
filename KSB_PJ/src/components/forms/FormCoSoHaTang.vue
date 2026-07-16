<script setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import AppToast from '@/components/AppToast.vue'
import { clearFormDraft, loadFormDraft, saveFormDraft, scrollFocusedFieldIntoView } from '@/utils/formDraftStore'
import { saveReport } from '@/utils/reportStore'
import { applyReportTemplate } from '@/utils/templateStore'

const router = useRouter()
const DRAFT_KEY = 'bb_csht'

const form = ref({
  ngayKiemTra: new Date().toISOString().split('T')[0],
  thanhPhans: [
    { stt: 1, hoTen: '', chucVu: '' }
  ],
  yKienBoPhanPhuTrach: '',
  yKienBoPhanCheBien: ''
})

const items = ref([
  { mucSo: 1, noiDung: 'Bếp bố trí theo nguyên tắc 1 chiều, tách biệt nguồn ô nhiễm', dat: null, ghiChu: '' },
  { mucSo: 2, noiDung: 'Tường, trần nhà, sàn nhà không thấm nước, rạn nứt, ẩm mốc', dat: null, ghiChu: '' },
  { mucSo: 3, noiDung: 'Hệ thống cung cấp nước', dat: null, ghiChu: '' },
  { mucSo: 4, noiDung: 'Hệ thống xử lý chất thải', dat: null, ghiChu: '' },
  { mucSo: 5, noiDung: 'Hệ thống bếp gas, bình gas', dat: null, ghiChu: '' },
  { mucSo: 6, noiDung: 'Hệ thống điện', dat: null, ghiChu: '' },
  { mucSo: 7, noiDung: 'Phòng cháy chữa cháy', dat: null, ghiChu: '' },
  { mucSo: 8, noiDung: 'Máy móc trang thiết bị được bảo trì, bảo dưỡng định kỳ', dat: null, ghiChu: '' },
  { mucSo: 9, noiDung: 'Thiết bị phòng chống côn trùng', dat: null, ghiChu: '' },
  { mucSo: 10, noiDung: 'Bồn rửa tay', dat: null, ghiChu: '' },
  { mucSo: 11, noiDung: 'Bàn ghế', dat: null, ghiChu: '' },
  { mucSo: 12, noiDung: 'Phương tiện vận chuyển thức ăn', dat: null, ghiChu: '' },
  { mucSo: 13, noiDung: 'Phương tiện thu gom', dat: null, ghiChu: '' }
])

applyReportTemplate('CoSoHaTang', items.value)

const isSubmitting = ref(false)
const toast = ref({ visible: false, message: '' })

let toastTimer = null

const completedCount = computed(() => items.value.filter(item => item.dat !== null).length)
const totalCount = computed(() => items.value.length)
const progressPercent = computed(() => Math.round((completedCount.value / totalCount.value) * 100))

const highlightUnchecked = ref(false)

const scrollToFirstUnchecked = () => {
  highlightUnchecked.value = true
  const uncheckedItem = items.value.find(item => item.dat === null)
  if (uncheckedItem) {
    const el = document.getElementById(`item-row-${uncheckedItem.mucSo}`)
    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'center' })
    }
  }
}

const showToast = message => {
  toast.value = { visible: true, message }
  window.clearTimeout(toastTimer)
  toastTimer = window.setTimeout(() => {
    toast.value.visible = false
  }, 4500)
}

const addThanhPhan = () => {
  form.value.thanhPhans.push({
    stt: form.value.thanhPhans.length + 1,
    hoTen: '',
    chucVu: ''
  })
}

const removeThanhPhan = index => {
  form.value.thanhPhans.splice(index, 1)
  form.value.thanhPhans.forEach((item, idx) => {
    item.stt = idx + 1
  })
}

const restoreDraft = () => {
  const draft = loadFormDraft(DRAFT_KEY)
  if (!draft) {
    return
  }

  if (draft.form) {
    const hasTypedData = draft.form.thanhPhans?.some(tp => tp.hoTen?.trim() !== '')
    if (!hasTypedData) {
      draft.form.thanhPhans = [{ stt: 1, hoTen: '', chucVu: '' }]
    }
    form.value = draft.form
  }

  if (Array.isArray(draft.items)) {
    items.value = draft.items
  }
}

const cancelForm = () => {
  clearFormDraft(DRAFT_KEY)
  router.push('/employee')
}

const submitForm = async () => {
  const unchecked = items.value.find(item => item.dat === null)
  if (unchecked) {
    showToast(`Vui lòng hoàn thành tiêu chí số ${unchecked.mucSo}!`)
    scrollToFirstUnchecked()
    return
  }

  isSubmitting.value = true

  try {
    await saveReport({
      soBienBan: `BB-CSHT-${Date.now().toString().slice(-4)}`,
      loaiBienBan: 'CoSoHaTang',
      ngayKiemTra: form.value.ngayKiemTra,
      thanhPhans: form.value.thanhPhans,
      chiTiets: items.value.map(item => ({
        mucSo: item.mucSo,
        phanNhom: '',
        noiDung: item.noiDung,
        dat: item.dat,
        ghiChu: item.ghiChu
      })),
      yKienBoPhanPhuTrach: form.value.yKienBoPhanPhuTrach,
      yKienBoPhanCheBien: form.value.yKienBoPhanCheBien,
      chuKys: []
    })

    clearFormDraft(DRAFT_KEY)
    showToast('Đã gửi biên bản cơ sở hạ tầng lên admin thành công!')
  } catch (error) {
    showToast(error.message || 'Không thể gửi biên bản.')
  } finally {
    isSubmitting.value = false
  }
}

onMounted(() => {
  restoreDraft()
})

watch(
  [form, items],
  () => {
    saveFormDraft(DRAFT_KEY, {
      form: form.value,
      items: items.value
    })
  },
  { deep: true }
)

onUnmounted(() => {
  window.clearTimeout(toastTimer)
})
</script>

<template>
  <div class="form-container">
    <AppToast :visible="toast.visible" :message="toast.message" />

    <div class="glass-card header-card">
      <h2>Biên bản kiểm tra Cơ sở hạ tầng, trang thiết bị</h2>
      <p class="subtitle">Tại bộ phận chế biến và cung cấp suất ăn</p>
    </div>

    <div class="form-content-wrapper">
      <form @submit.prevent="submitForm">
        <div class="glass-card section-card">
          <h3>Thông tin chung</h3>
          <div class="form-row">
            <div class="form-group">
              <label>Ngày kiểm tra</label>
              <input v-model="form.ngayKiemTra" type="date" class="glass-input" required />
            </div>
          </div>

          <div class="thanh-phan-list">
            <div class="section-topline">
              <h4>Thành phần</h4>
              <button type="button" class="btn-outline" @click="addThanhPhan">+ Thêm người</button>
            </div>

            <div v-for="(tp, index) in form.thanhPhans" :key="`${tp.stt}-${index}`" class="thanh-phan-item">
              <div class="stt-badge">{{ tp.stt }}</div>
              <input v-model="tp.hoTen" type="text" placeholder="Họ và tên" class="glass-input flex-1" required />
              <input v-model="tp.chucVu" type="text" placeholder="Chức vụ" class="glass-input flex-1" required />
              <button v-if="form.thanhPhans.length > 1" type="button" class="btn-icon text-red" @click="removeThanhPhan(index)">
                <ion-icon name="trash-outline"></ion-icon>
              </button>
            </div>
          </div>
        </div>

        <div class="glass-card section-card">
          <h3>Nội dung kiểm tra</h3>
          <div class="table-responsive">
            <table class="glass-table">
              <thead>
                <tr>
                  <th>TT</th>
                  <th>Nội dung kiểm tra</th>
                  <th>Đạt</th>
                  <th>Không đạt</th>
                  <th>Ghi chú</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in items" :key="item.mucSo" :id="'item-row-' + item.mucSo" :class="{ 'row-missing-highlight': highlightUnchecked && item.dat === null }">
                  <td class="text-center">{{ item.mucSo }}</td>
                  <td>{{ item.noiDung }}</td>
                  <td class="text-center"><input v-model="item.dat" type="radio" :name="`csht_${item.mucSo}`" :value="true" /></td>
                  <td class="text-center"><input v-model="item.dat" type="radio" :name="`csht_${item.mucSo}`" :value="false" /></td>
                  <td>
                    <textarea v-model="item.ghiChu" rows="2" class="glass-input-sm note-input" placeholder="Nhập ghi chú..." @focus="scrollFocusedFieldIntoView"></textarea>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div class="glass-card section-card">
          <h3>Ý kiến xác nhận</h3>
          <div class="signature-grid">
            <div class="form-group">
              <label>Bộ phận phụ trách</label>
              <textarea v-model="form.yKienBoPhanPhuTrach" rows="4" class="glass-input" @focus="scrollFocusedFieldIntoView"></textarea>
            </div>
            <div class="form-group">
              <label>Bộ phận chế biến và cung cấp suất ăn</label>
              <textarea v-model="form.yKienBoPhanCheBien" rows="4" class="glass-input" @focus="scrollFocusedFieldIntoView"></textarea>
            </div>
          </div>
        </div>

        <div class="form-actions">
          <button type="button" class="btn-secondary" @click="cancelForm">Hủy</button>
          <button type="submit" class="btn-primary" :disabled="isSubmitting">
            <span v-if="!isSubmitting"><ion-icon name="send-outline"></ion-icon> Gửi biên bản lên admin</span>
            <span v-else class="spinner"></span>
          </button>
        </div>
      </form>

      <!-- Sticky Progress Bar (Floating Vertical Gauge on Desktop, Horizontal Pill on Mobile) -->
      <div class="sticky-progress-bar">
        <div class="progress-info">
          <!-- Percentage Badge -->
          <div class="progress-percent-circle">
            <span>{{ progressPercent }}%</span>
          </div>
          
          <!-- Vertical/Horizontal Track -->
          <div class="progress-track">
            <div class="progress-fill" :style="{ '--progress-val': progressPercent + '%' }"></div>
          </div>
          
          <!-- Text/Fraction -->
          <div class="progress-text-fraction">
            <strong>{{ completedCount }}/{{ totalCount }}</strong>
            <span>tiêu chí</span>
          </div>
          
          <!-- Action Button -->
          <button v-if="completedCount < totalCount" type="button" class="btn-goto-missing" title="Tìm mục chưa tích" @click="scrollToFirstUnchecked">
            <ion-icon name="search-outline"></ion-icon>
            <span class="btn-text">Tìm mục chưa tích</span>
            <span class="btn-tooltip">Tìm mục chưa tích</span>
          </button>
          <div v-else class="progress-success-icon" title="Đã hoàn thành">
            <ion-icon name="checkmark-circle"></ion-icon>
            <span class="btn-text">Đã hoàn thành</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.form-container { display: flex; flex-direction: column; gap: 20px; max-width: 1200px; margin: 0 auto; padding-bottom: 120px !important; }
.form-container form { display: flex; flex-direction: column; gap: 20px; }

/* Wrapper for form content side-by-side with sticky progress bar on desktop */
@media (min-width: 961px) {
  .form-content-wrapper {
    display: flex;
    flex-direction: row;
    align-items: flex-start;
    gap: 24px;
    position: relative;
    width: 100%;
  }
  .form-content-wrapper > form {
    flex: 1;
    min-width: 0;
  }
  .sticky-progress-bar {
    position: sticky !important;
    top: 24px;
    right: 0;
    transform: none !important;
    width: 72px;
    background: rgba(255, 255, 255, 0.95);
    backdrop-filter: blur(12px);
    -webkit-backdrop-filter: blur(12px);
    border: 1px solid rgba(148, 163, 184, 0.35);
    padding: 20px 10px;
    box-shadow: 0 12px 40px rgba(15, 23, 42, 0.12);
    z-index: 90;
    border-radius: 40px;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 16px;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    flex-shrink: 0;
  }
  .sticky-progress-bar:hover {
    transform: scale(1.03) !important;
    box-shadow: 0 16px 45px rgba(15, 23, 42, 0.18);
  }
}

@media (max-width: 960px) {
  .form-content-wrapper {
    display: block;
    width: 100%;
  }
  .sticky-progress-bar {
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    top: auto;
    width: 100%;
    transform: none;
    border-radius: 16px 16px 0 0;
    border: none;
    border-top: 1px solid #e2e8f0;
    padding: 12px 20px;
    box-shadow: 0 -5px 25px rgba(0, 0, 0, 0.08);
    flex-direction: row;
    justify-content: space-between;
    height: auto;
    gap: 0;
    z-index: 999;
    background: rgba(255, 255, 255, 0.95);
    backdrop-filter: blur(12px);
    -webkit-backdrop-filter: blur(12px);
    display: flex;
    align-items: center;
  }
}

.progress-info {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 14px;
  width: 100%;
}

.progress-percent-circle {
  width: 52px;
  height: 52px;
  border-radius: 50%;
  background: linear-gradient(135deg, #f0fdf4, #e0f2fe);
  border: 1px solid rgba(56, 189, 248, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 10px rgba(15, 23, 42, 0.04);
}

.progress-percent-circle span {
  font-size: 0.95rem;
  font-weight: 800;
  color: #0369a1;
}

.progress-track {
  width: 8px;
  height: 120px;
  background: #f1f5f9;
  border-radius: 999px;
  overflow: hidden;
  position: relative;
  display: flex;
  align-items: flex-end;
}

.progress-fill {
  width: 100%;
  height: var(--progress-val);
  background: linear-gradient(180deg, #22c55e, #38bdf8);
  border-radius: 999px;
  transition: height 0.4s cubic-bezier(0.4, 0, 0.2, 1);
}

.progress-text-fraction {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  text-align: center;
}

.progress-text-fraction strong {
  font-size: 0.95rem;
  color: #0f172a;
  font-weight: 800;
}

.progress-text-fraction span {
  font-size: 0.72rem;
  color: #64748b;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.btn-goto-missing {
  width: 44px;
  height: 44px;
  border-radius: 50%;
  background: #eff6ff;
  color: #0284c7;
  border: 1px solid #bae6fd;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
  cursor: pointer;
  position: relative;
  transition: all 0.2s ease;
  box-shadow: 0 4px 12px rgba(2, 132, 199, 0.15);
}

.btn-goto-missing:hover {
  background: #0284c7;
  color: #ffffff;
  border-color: #0284c7;
  transform: scale(1.05);
}

.btn-goto-missing .btn-tooltip {
  position: absolute;
  right: 60px;
  top: 50%;
  transform: translateY(-50%);
  background: #0f172a;
  color: #ffffff;
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 0.8rem;
  font-weight: 600;
  white-space: nowrap;
  opacity: 0;
  pointer-events: none;
  transition: all 0.2s ease;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}

.btn-goto-missing .btn-tooltip::after {
  content: '';
  position: absolute;
  left: 100%;
  top: 50%;
  transform: translateY(-50%);
  border-width: 5px;
  border-style: solid;
  border-color: transparent transparent transparent #0f172a;
}

.btn-goto-missing:hover .btn-tooltip {
  opacity: 1;
  right: 54px;
}

.progress-success-icon {
  width: 44px;
  height: 44px;
  border-radius: 50%;
  background: #f0fdf4;
  color: #16a34a;
  border: 1px solid #bbf7d0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  box-shadow: 0 4px 12px rgba(22, 163, 74, 0.15);
}

.btn-goto-missing .btn-text,
.progress-success-icon .btn-text {
  display: none;
}

/* Mobile responsive */
@media (max-width: 960px) {
  .sticky-progress-bar {
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    top: auto;
    width: 100%;
    transform: none;
    border-radius: 16px 16px 0 0;
    border: none;
    border-top: 1px solid #e2e8f0;
    padding: 12px 20px;
    box-shadow: 0 -5px 25px rgba(0, 0, 0, 0.08);
    flex-direction: row;
    justify-content: space-between;
    height: auto;
    gap: 0;
  }
  
  .sticky-progress-bar:hover {
    transform: none;
  }
  
  .progress-info {
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    width: auto;
  }
  
  .progress-percent-circle {
    width: 38px;
    height: 38px;
    flex-shrink: 0;
  }
  
  .progress-percent-circle span {
    font-size: 0.8rem;
  }
  
  .progress-track {
    display: none;
  }
  
  .progress-text-fraction {
    flex-direction: row;
    align-items: center;
    gap: 4px;
  }
  
  .progress-text-fraction strong {
    font-size: 0.9rem;
  }
  
  .progress-text-fraction span {
    font-size: 0.8rem;
  }
  
  .btn-goto-missing, .progress-success-icon {
    width: auto;
    height: auto;
    border-radius: 10px;
    padding: 8px 16px;
    font-size: 0.85rem;
    font-weight: 700;
    box-shadow: none;
  }
  
  .btn-goto-missing ion-icon, .progress-success-icon ion-icon {
    font-size: 1.1rem;
  }
  
  .btn-goto-missing .btn-tooltip {
    display: none;
  }
  
  .btn-goto-missing .btn-text,
  .progress-success-icon .btn-text {
    display: inline;
    font-size: 0.85rem;
    margin-left: 6px;
  }
}



/* Flash Highlight Animation for incomplete item row */
:global(tr.flash-highlight) {
  animation: rowFlash 1.6s ease-in-out infinite;
}

:global(tr.row-missing-highlight td) {
  background-color: #fee2e2 !important;
  border-top: 1.5px solid #f87171 !important;
  border-bottom: 1.5px solid #f87171 !important;
}
:global(tr.row-missing-highlight td:first-child) {
  border-left: 3px solid #ef4444 !important;
}
:global(tr.row-missing-highlight td:last-child) {
  border-right: 1.5px solid #f87171 !important;
}

@keyframes rowFlash {
  0%, 100% {
    background-color: transparent;
  }
  50% {
    background-color: #fee2e2;
    box-shadow: inset 0 0 0 2px #ef4444;
  }
}
.glass-card { background: #ffffff; border: 1px solid #e2e8f0; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05), 0 2px 4px -1px rgba(0,0,0,0.03); border-radius: 12px; padding: 24px; color: #334155; }
.header-card { text-align: center; background: linear-gradient(135deg, #e0f2fe, #dbeafe); border-bottom: 3px solid #0284c7; }
.header-card h2 { margin: 0 0 10px; font-size: 1.8rem; color: #0f172a; }
.subtitle { color: #475569; font-weight: 600; letter-spacing: 1px; text-transform: uppercase; }
.section-card h3 { margin: 0 0 20px; padding-bottom: 10px; border-bottom: 1px solid #e2e8f0; color: #0f172a; }
.thanh-phan-list { margin-top: 24px; padding-top: 20px; border-top: 1px solid #e2e8f0; }
.section-topline { display: flex; justify-content: space-between; align-items: center; margin-bottom: 14px; }
.form-row, .signature-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; min-width: 0; }
.form-group { display: flex; flex-direction: column; gap: 8px; min-width: 0; }
.form-group label { font-size: 0.9rem; font-weight: 600; color: #475569; }
.glass-input, .glass-input-sm { width: 100%; max-width: 100%; box-sizing: border-box; background: #f8fafc; border: 1px solid #cbd5e1; border-radius: 8px; color: #1e293b; padding: 12px 16px; font-family: inherit; font-size: 0.95rem; }
.glass-input-sm { padding: 8px 12px; font-size: 0.85rem; width: 100%; }
.note-input { display: block; min-height: 44px; line-height: 1.45; resize: vertical; white-space: pre-wrap; overflow-wrap: anywhere; }
.thanh-phan-item { display: flex; gap: 12px; margin-bottom: 12px; align-items: center; padding: 12px; border-radius: 10px; background: #f8fafc; border: 1px solid #e2e8f0; }
.stt-badge { width: 36px; height: 36px; display: flex; align-items: center; justify-content: center; background: #eff6ff; color: #0369a1; border-radius: 50%; font-weight: 700; flex-shrink: 0; }
.flex-1 { flex: 1; }
.btn-icon { background: transparent; border: none; font-size: 1.5rem; cursor: pointer; display: flex; align-items: center; justify-content: center; }
.text-red { color: #ef4444; }
.btn-outline { background: transparent; border: 1px solid #94a3b8; color: #475569; padding: 6px 12px; border-radius: 6px; cursor: pointer; font-size: 0.85rem; }
.table-responsive { overflow-x: auto; }
.glass-table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
.glass-table th, .glass-table td { padding: 12px; border-bottom: 1px solid #e2e8f0; vertical-align: middle; }
.glass-table th { background: #f8fafc; text-align: left; font-weight: 600; color: #475569; }
.text-center { text-align: center !important; }
.form-actions { display: flex; justify-content: flex-end; gap: 15px; }
.btn-primary, .btn-secondary { padding: 12px 24px; border-radius: 8px; font-size: 1rem; font-weight: 600; cursor: pointer; display: flex; align-items: center; gap: 8px; border: none; }
.btn-primary { background: #0ea5e9; color: white; }
.btn-secondary { background: #f1f5f9; color: #475569; border: 1px solid #cbd5e1; }
.spinner { width: 20px; height: 20px; border: 3px solid rgba(255,255,255,0.3); border-top-color: #fff; border-radius: 50%; animation: spin 0.8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 768px) {
  .form-container { padding-bottom: max(320px, env(safe-area-inset-bottom)); }
  .form-row, .signature-grid { grid-template-columns: minmax(0, 1fr); width: 100%; max-width: 100%; }
  .glass-card { overflow: hidden; }
  input[type='date'].glass-input { width: 100%; max-width: 100%; min-width: 0; -webkit-appearance: none; appearance: none; }
  .thanh-phan-item { flex-direction: column; align-items: stretch; gap: 8px; }
  .table-responsive { overflow-x: auto; -webkit-overflow-scrolling: touch; padding-bottom: 8px; }
  .glass-table { min-width: 760px; }
  .note-input { min-width: 180px; min-height: 72px; font-size: 0.95rem; }
  .glass-input:focus, .glass-input-sm:focus { border-color: #38bdf8; box-shadow: 0 0 0 3px rgba(14, 165, 233, 0.18); outline: none; }
}
</style>
