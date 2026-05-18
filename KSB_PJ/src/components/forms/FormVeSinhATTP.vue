<script setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import AppToast from '@/components/AppToast.vue'
import { clearFormDraft, loadFormDraft, saveFormDraft, scrollFocusedFieldIntoView } from '@/utils/formDraftStore'
import { saveReport } from '@/utils/reportStore'
import { applyReportTemplate } from '@/utils/templateStore'

const router = useRouter()
const DRAFT_KEY = 'bb_vesinh'

const form = ref({
  ngayKiemTra: new Date().toISOString().split('T')[0],
  thanhPhans: [
    { stt: 1, hoTen: '', chucVu: 'Đại diện Khoa Dinh Dưỡng' },
    { stt: 2, hoTen: '', chucVu: 'Đại diện BPCB & CCSA' },
    { stt: 3, hoTen: '', chucVu: 'Nhân viên giám sát' }
  ],
  gopYKhoaDinhDuong: '',
  yKienBPCB: ''
})

const group = (id, title, items) => ({ id, title, items })
const item = (mucSo, noiDung) => ({ mucSo, noiDung, dat: null, ghiChu: '' })

const sections = ref([
  group('I', 'Điều kiện về con người', [
    item(1, 'Nhân viên mặc đồng phục đúng quy định, gọn gàng, sạch sẽ'),
    item(2, 'Móng tay cắt ngắn, không mang trang sức sai quy định'),
    item(3, 'Rửa tay bằng xà phòng trước khi chế biến và sau khi đi vệ sinh')
  ]),
  group('II', 'Kiểm tra dụng cụ', [
    item(4, 'Dụng cụ chế biến thực phẩm sống/chín riêng biệt'),
    item(5, 'Dụng cụ chứa đựng thực phẩm sạch sẽ, nguyên vẹn, có nắp đậy')
  ]),
  group('III', 'Giám sát môi trường bếp ăn', [
    item(6, 'Không gian bếp sạch sẽ, không tồn đọng nước thải/rác thải'),
    item(7, 'Kho thực phẩm được sắp xếp đúng nguyên tắc nhập trước xuất trước')
  ])
])

sections.value.forEach(section => {
  applyReportTemplate('VeSinh', section.items, `${section.id}. ${section.title}`)
})

const activeSectionIndex = ref(0)
const activeSection = computed(() => sections.value[activeSectionIndex.value])
const totalItems = computed(() => sections.value.reduce((sum, section) => sum + section.items.length, 0))
const isSubmitting = ref(false)
const toast = ref({ visible: false, message: '' })

let toastTimer = null

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
  if (!draft) return
  if (draft.form) form.value = draft.form
  if (Array.isArray(draft.sections)) sections.value = draft.sections
  if (Number.isInteger(draft.activeSectionIndex)) activeSectionIndex.value = draft.activeSectionIndex
}

const cancelForm = () => {
  clearFormDraft(DRAFT_KEY)
  router.push('/employee')
}

const submitForm = async () => {
  isSubmitting.value = true

  const chiTiets = sections.value.flatMap(section => {
    return section.items.map(row => ({
      mucSo: row.mucSo,
      phanNhom: `${section.id}. ${section.title}`,
      noiDung: row.noiDung,
      dat: row.dat,
      ghiChu: row.ghiChu
    }))
  })

  try {
    await saveReport({
      soBienBan: `BB-VSATTP-${Date.now().toString().slice(-4)}`,
      loaiBienBan: 'VeSinh',
      ngayKiemTra: form.value.ngayKiemTra,
      gopYKhoaDinhDuong: form.value.gopYKhoaDinhDuong,
      yKienBPCB: form.value.yKienBPCB,
      thanhPhans: form.value.thanhPhans,
      chiTiets,
      chuKys: []
    })

    clearFormDraft(DRAFT_KEY)
    showToast('Đã gửi biên bản lên admin thành công!')
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
  [form, sections, activeSectionIndex],
  () => {
    saveFormDraft(DRAFT_KEY, {
      form: form.value,
      sections: sections.value,
      activeSectionIndex: activeSectionIndex.value
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
      <h2>Biên bản kiểm tra Vệ sinh An toàn Thực phẩm</h2>
      <p class="subtitle">Khoa Dinh Dưỡng</p>
    </div>

    <form @submit.prevent="submitForm">
      <div class="glass-card section-card">
        <h3>Thông tin chung</h3>
        <div class="form-row">
          <div class="form-group">
            <label>Ngày kiểm tra</label>
            <input v-model="form.ngayKiemTra" type="date" required class="glass-input" />
          </div>
        </div>

        <div class="thanh-phan-list">
          <div class="section-topline">
            <h4>Thành phần đoàn kiểm tra</h4>
            <button type="button" class="btn-outline" @click="addThanhPhan">+ Thêm người</button>
          </div>

          <div v-for="(tp, index) in form.thanhPhans" :key="`${tp.stt}-${index}`" class="thanh-phan-item">
            <div class="stt-badge">{{ tp.stt }}</div>
            <input v-model="tp.hoTen" type="text" placeholder="Họ và tên" class="glass-input flex-1" required />
            <input v-model="tp.chucVu" type="text" placeholder="Chức vụ/Đại diện" class="glass-input flex-1" required />
            <button v-if="form.thanhPhans.length > 1" type="button" class="btn-icon text-red" @click="removeThanhPhan(index)">
              <ion-icon name="trash-outline"></ion-icon>
            </button>
          </div>
        </div>
      </div>

      <div class="glass-card section-card">
        <div class="content-header">
          <div>
            <h3>Nội dung kiểm tra</h3>
            <p>{{ totalItems }} tiêu chí, chia theo {{ sections.length }} phần.</p>
          </div>
          <div class="pager-counter">Phần {{ activeSectionIndex + 1 }}/{{ sections.length }}</div>
        </div>

        <div class="section-tabs">
          <button
            v-for="(section, index) in sections"
            :key="section.id"
            type="button"
            class="section-tab"
            :class="{ active: index === activeSectionIndex }"
            @click="activeSectionIndex = index"
          >
            <span>{{ section.id }}</span>
            <strong>{{ section.title }}</strong>
          </button>
        </div>

        <div class="table-responsive">
          <table class="glass-table">
            <thead>
              <tr>
                <th>STT</th>
                <th>Nội dung kiểm tra</th>
                <th>Đạt</th>
                <th>K.Đạt</th>
                <th>Ghi chú</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in activeSection.items" :key="row.mucSo">
                <td class="text-center">{{ row.mucSo }}</td>
                <td>{{ row.noiDung }}</td>
                <td class="text-center"><input v-model="row.dat" type="radio" :name="`vsattp_${row.mucSo}`" :value="true" /></td>
                <td class="text-center"><input v-model="row.dat" type="radio" :name="`vsattp_${row.mucSo}`" :value="false" /></td>
                <td>
                  <textarea v-model="row.ghiChu" rows="2" placeholder="Nhập ghi chú..." class="glass-input-sm note-input" @focus="scrollFocusedFieldIntoView"></textarea>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="section-pager">
          <button type="button" class="btn-secondary" :disabled="activeSectionIndex === 0" @click="activeSectionIndex -= 1">Phần trước</button>
          <button type="button" class="btn-secondary" :disabled="activeSectionIndex === sections.length - 1" @click="activeSectionIndex += 1">Phần sau</button>
        </div>
      </div>

      <div class="glass-card section-card">
        <h3>Ý kiến - Đề xuất</h3>
        <div class="form-group">
          <label>Nhắc nhở, góp ý của Khoa Dinh Dưỡng</label>
          <textarea v-model="form.gopYKhoaDinhDuong" rows="4" class="glass-input" @focus="scrollFocusedFieldIntoView"></textarea>
        </div>
        <div class="form-group">
          <label>Ý kiến của Bộ phận CB & CCSA</label>
          <textarea v-model="form.yKienBPCB" rows="4" class="glass-input" @focus="scrollFocusedFieldIntoView"></textarea>
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
  </div>
</template>

<style scoped>
.form-container { display: flex; flex-direction: column; gap: 20px; max-width: 1200px; margin: 0 auto; padding-bottom: 50px; }
.form-container form { display: flex; flex-direction: column; gap: 20px; }
.glass-card { background: #ffffff; border: 1px solid #e2e8f0; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05), 0 2px 4px -1px rgba(0,0,0,0.03); border-radius: 12px; padding: 24px; color: #334155; }
.header-card { text-align: center; background: linear-gradient(135deg, #e0f2fe, #dcfce7); border-bottom: 3px solid #0ea5e9; }
.header-card h2 { margin: 0 0 10px; font-size: 1.8rem; color: #0f172a; }
.subtitle { color: #475569; font-weight: 600; letter-spacing: 1px; text-transform: uppercase; }
.section-card h3 { margin: 0 0 20px; padding-bottom: 10px; border-bottom: 1px solid #e2e8f0; color: #0f172a; }
.content-header, .section-topline { display: flex; justify-content: space-between; align-items: center; gap: 16px; }
.form-row { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; }
.form-group { display: flex; flex-direction: column; gap: 8px; margin-bottom: 16px; }
.glass-input, .glass-input-sm { width: 100%; max-width: 100%; box-sizing: border-box; background: #f8fafc; border: 1px solid #cbd5e1; border-radius: 8px; color: #1e293b; padding: 12px 16px; font-family: inherit; font-size: 0.95rem; }
.glass-input-sm { padding: 8px 12px; font-size: 0.85rem; width: 100%; }
.note-input { display: block; min-height: 44px; line-height: 1.45; resize: vertical; white-space: pre-wrap; overflow-wrap: anywhere; }
.thanh-phan-item { display: flex; gap: 15px; margin-bottom: 15px; align-items: center; }
.stt-badge { width: 36px; height: 36px; display: flex; align-items: center; justify-content: center; background: #eff6ff; color: #0369a1; border-radius: 50%; font-weight: 700; flex-shrink: 0; }
.flex-1 { flex: 1; }
.btn-icon { background: transparent; border: none; font-size: 1.5rem; cursor: pointer; display: flex; }
.text-red { color: #ef4444; }
.btn-outline { background: transparent; border: 1px solid #94a3b8; color: #475569; padding: 6px 12px; border-radius: 6px; cursor: pointer; font-size: 0.85rem; }
.section-tabs { display: grid; grid-template-columns: repeat(auto-fit, minmax(170px, 1fr)); gap: 10px; margin-bottom: 18px; }
.section-tab { display: flex; align-items: center; gap: 10px; padding: 10px 12px; border: 1px solid #cbd5e1; border-radius: 10px; background: #f8fafc; cursor: pointer; text-align: left; }
.section-tab span { display: inline-flex; align-items: center; justify-content: center; width: 32px; height: 32px; border-radius: 50%; background: #e0f2fe; color: #0369a1; font-weight: 800; }
.section-tab.active { border-color: #0ea5e9; background: #eff6ff; }
.table-responsive { overflow-x: auto; }
.glass-table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
.glass-table th, .glass-table td { padding: 12px; border-bottom: 1px solid #e2e8f0; vertical-align: middle; }
.glass-table th { background: #f8fafc; text-align: left; font-weight: 600; color: #475569; }
.text-center { text-align: center !important; }
.section-pager, .form-actions { display: flex; justify-content: flex-end; gap: 15px; margin-top: 16px; }
.btn-primary, .btn-secondary { padding: 12px 24px; border-radius: 8px; font-size: 1rem; font-weight: 600; cursor: pointer; display: flex; align-items: center; gap: 8px; border: none; }
.btn-primary { background: #0ea5e9; color: white; }
.btn-secondary { background: #f1f5f9; color: #475569; border: 1px solid #cbd5e1; }
.spinner { width: 20px; height: 20px; border: 3px solid rgba(255,255,255,0.3); border-top-color: #fff; border-radius: 50%; animation: spin 0.8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 768px) {
  .form-container { padding-bottom: max(320px, env(safe-area-inset-bottom)); }
  .form-row { grid-template-columns: 1fr; }
  .thanh-phan-item, .content-header, .section-pager, .form-actions { flex-direction: column; align-items: stretch; }
  .table-responsive { overflow-x: auto; -webkit-overflow-scrolling: touch; padding-bottom: 8px; }
  .glass-table { min-width: 760px; }
  .note-input { min-width: 180px; min-height: 72px; font-size: 0.95rem; }
  .glass-input:focus, .glass-input-sm:focus { border-color: #38bdf8; box-shadow: 0 0 0 3px rgba(14, 165, 233, 0.18); outline: none; }
}
</style>
