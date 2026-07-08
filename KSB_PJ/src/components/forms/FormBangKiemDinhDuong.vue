<script setup>
import { onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import AppToast from '@/components/AppToast.vue'
import { clearFormDraft, loadFormDraft, saveFormDraft, scrollFocusedFieldIntoView } from '@/utils/formDraftStore'
import { saveReport } from '@/utils/reportStore'
import { applyReportTemplate } from '@/utils/templateStore'

const router = useRouter()
const DRAFT_KEY = 'bb_bangkiem_dinhduong'

const form = ref({
  ngayKiemTra: new Date().toISOString().split('T')[0],
  uuDiem: '',
  tonTai: ''
})

const thanhPhans = ref([
  { stt: 1, hoTen: 'Vương Thị Thu Huyền', chucVu: 'Nhân viên Dinh dưỡng', nhom: 'Đại diện khoa dinh dưỡng' },
  { stt: 2, hoTen: '', chucVu: '', nhom: 'Đại diện khoa/phòng' },
  { stt: 3, hoTen: '', chucVu: '', nhom: 'Đại diện khoa/phòng' },
  { stt: 4, hoTen: '', chucVu: '', nhom: 'Đại diện khoa/phòng' }
])

const rows = ref([
  { mucSo: 1, noiDung: 'Hồ sơ bệnh án có dán phiếu ĐGTTDD', dat: null, ghiChu: '' },
  { mucSo: 2, noiDung: 'Cân, đo, tính BMI (điều dưỡng)', dat: null, ghiChu: '' },
  { mucSo: 3, noiDung: 'Đánh giá tình trạng dinh dưỡng (bác sĩ)', dat: null, ghiChu: '' },
  { mucSo: 4, noiDung: 'Mã chế độ ăn (bác sĩ)', dat: null, ghiChu: '' },
  { mucSo: 5, noiDung: 'Bác sĩ, điều dưỡng hướng dẫn chế độ ăn người bệnh (phỏng vấn người bệnh)', dat: null, ghiChu: '' },
  { mucSo: 6, noiDung: 'Nắm được thông tư 18/BYT-2011', dat: null, ghiChu: '' },
  { mucSo: 7, noiDung: 'Trang thiết bị (thước dây, cân)', dat: null, ghiChu: '' },
  { mucSo: 8, noiDung: 'Ý kiến về suất ăn', dat: null, ghiChu: '' }
])

applyReportTemplate('BangKiemDinhDuong', rows.value)

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
  thanhPhans.value.push({
    stt: thanhPhans.value.length + 1,
    hoTen: '',
    chucVu: '',
    nhom: 'Đại diện khoa/phòng'
  })
}

const removeThanhPhan = index => {
  if (thanhPhans.value.length <= 1) return
  thanhPhans.value.splice(index, 1)
  thanhPhans.value.forEach((item, idx) => {
    item.stt = idx + 1
  })
}

const restoreDraft = () => {
  const draft = loadFormDraft(DRAFT_KEY)
  if (!draft) return
  if (draft.form) form.value = { ...form.value, ...draft.form }
  if (Array.isArray(draft.thanhPhans)) thanhPhans.value = draft.thanhPhans
  if (Array.isArray(draft.rows)) rows.value = draft.rows
}

const cancelForm = () => {
  clearFormDraft(DRAFT_KEY)
  router.push('/employee')
}

const submitForm = async () => {
  isSubmitting.value = true

  try {
    await saveReport({
      soBienBan: `BB-BKDD-${Date.now().toString().slice(-4)}`,
      loaiBienBan: 'BangKiemDinhDuong',
      ngayKiemTra: form.value.ngayKiemTra,
      thanhPhans: thanhPhans.value
        .filter(item => item.hoTen?.trim() || item.chucVu?.trim())
        .map((item, index) => ({
          stt: index + 1,
          hoTen: item.hoTen,
          chucVu: item.chucVu || item.nhom || ''
        })),
      chiTiets: [
        ...rows.value.map(item => ({
          mucSo: item.mucSo,
          phanNhom: 'II. Nội dung và kết quả kiểm tra',
          noiDung: item.noiDung,
          dat: item.dat,
          ghiChu: item.ghiChu
        })),
        {
          mucSo: 9,
          phanNhom: 'III. Kết luận, kiến nghị và xử lý',
          noiDung: 'Các nội dung ưu điểm',
          dat: null,
          ghiChu: form.value.uuDiem
        },
        {
          mucSo: 10,
          phanNhom: 'III. Kết luận, kiến nghị và xử lý',
          noiDung: 'Các mặt còn tồn tại',
          dat: null,
          ghiChu: form.value.tonTai
        }
      ],
      gopYKhoaDinhDuong: form.value.uuDiem,
      yKienBPCB: form.value.tonTai,
      chuKys: []
    })

    clearFormDraft(DRAFT_KEY)
    showToast('Đã gửi bảng kiểm dinh dưỡng lên admin thành công!')
  } catch (error) {
    showToast(error.message || 'Không thể gửi bảng kiểm.')
  } finally {
    isSubmitting.value = false
  }
}

onMounted(() => {
  restoreDraft()
})

watch(
  [form, thanhPhans, rows],
  () => {
    saveFormDraft(DRAFT_KEY, {
      form: form.value,
      thanhPhans: thanhPhans.value,
      rows: rows.value
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
      <h2>Bảng kiểm dinh dưỡng</h2>
      <p class="subtitle">Thành phần tham gia buổi làm việc và nội dung đánh giá</p>
    </div>

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
            <h4>Thành phần tham gia</h4>
            <button type="button" class="btn-outline" @click="addThanhPhan">+ Thêm người</button>
          </div>

          <div v-for="(tp, index) in thanhPhans" :key="`${tp.stt}-${index}`" class="thanh-phan-item">
            <div class="stt-badge">{{ tp.stt }}</div>
            <input v-model="tp.nhom" type="text" placeholder="Nhóm đại diện" class="glass-input flex-1" />
            <input v-model="tp.hoTen" type="text" placeholder="Họ và tên" class="glass-input flex-1" />
            <input v-model="tp.chucVu" type="text" placeholder="Chức vụ" class="glass-input flex-1" />
            <button v-if="thanhPhans.length > 1" type="button" class="btn-icon text-red" @click="removeThanhPhan(index)">
              <ion-icon name="trash-outline"></ion-icon>
            </button>
          </div>
        </div>
      </div>

      <div class="glass-card section-card">
        <h3>Nội dung và kết quả kiểm tra</h3>
        <div class="table-responsive">
          <table class="glass-table">
            <thead>
              <tr>
                <th>STT</th>
                <th>Nội dung đánh giá</th>
                <th>Có</th>
                <th>Không</th>
                <th>Ghi chú</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in rows" :key="row.mucSo">
                <td class="text-center">{{ row.mucSo }}</td>
                <td>{{ row.noiDung }}</td>
                <td class="text-center"><input v-model="row.dat" type="radio" :name="`bkdd_${row.mucSo}`" :value="true" /></td>
                <td class="text-center"><input v-model="row.dat" type="radio" :name="`bkdd_${row.mucSo}`" :value="false" /></td>
                <td>
                  <textarea
                    v-model="row.ghiChu"
                    rows="2"
                    class="glass-input-sm note-input"
                    :placeholder="row.mucSo === 8 ? 'VD: Hài lòng / Không hài lòng' : 'Nhập ghi chú...'"
                    @focus="scrollFocusedFieldIntoView"
                  ></textarea>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div class="glass-card section-card">
        <h3>Kết luận, kiến nghị và xử lý</h3>
        <div class="signature-grid">
          <div class="form-group">
            <label>Các nội dung ưu điểm</label>
            <textarea v-model="form.uuDiem" rows="5" class="glass-input note-input" @focus="scrollFocusedFieldIntoView"></textarea>
          </div>
          <div class="form-group">
            <label>Các mặt còn tồn tại</label>
            <textarea v-model="form.tonTai" rows="5" class="glass-input note-input" @focus="scrollFocusedFieldIntoView"></textarea>
          </div>
        </div>
      </div>

      <div class="glass-card section-card">
        <h3>Vị trí ký khi in bảng kiểm</h3>
        <div class="print-signature-grid">
          <div>Đại diện khoa/phòng</div>
          <div>Khoa Dinh dưỡng</div>
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
.header-card { text-align: center; background: linear-gradient(135deg, #f0fdf4, #dcfce7); border-bottom: 3px solid #22c55e; }
.header-card h2 { margin: 0 0 10px; font-size: 1.8rem; color: #0f172a; }
.subtitle { color: #475569; font-weight: 600; letter-spacing: 1px; text-transform: uppercase; }
.section-card h3 { margin: 0 0 20px; padding-bottom: 10px; border-bottom: 1px solid #e2e8f0; color: #0f172a; }
.thanh-phan-list { margin-top: 24px; padding-top: 20px; border-top: 1px solid #e2e8f0; }
.section-topline { display: flex; justify-content: space-between; align-items: center; margin-bottom: 14px; }
.form-row, .signature-grid, .print-signature-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; min-width: 0; }
.form-group { display: flex; flex-direction: column; gap: 8px; min-width: 0; }
.form-group label { font-size: 0.9rem; font-weight: 600; color: #475569; }
.glass-input, .glass-input-sm { width: 100%; max-width: 100%; box-sizing: border-box; background: #f8fafc; border: 1px solid #cbd5e1; border-radius: 8px; color: #1e293b; padding: 12px 16px; font-family: inherit; font-size: 0.95rem; }
.glass-input-sm { padding: 8px 12px; font-size: 0.85rem; width: 100%; }
.note-input { display: block; min-height: 72px; line-height: 1.45; resize: vertical; white-space: pre-wrap; overflow-wrap: anywhere; }
.thanh-phan-item { display: grid; grid-template-columns: 36px minmax(160px, 0.8fr) minmax(0, 1fr) minmax(0, 1fr) 38px; gap: 12px; margin-bottom: 12px; align-items: center; padding: 12px; border-radius: 10px; background: #f8fafc; border: 1px solid #e2e8f0; }
.stt-badge { width: 36px; height: 36px; display: flex; align-items: center; justify-content: center; background: #f0fdf4; color: #15803d; border-radius: 50%; font-weight: 700; flex-shrink: 0; }
.flex-1 { min-width: 0; }
.btn-icon { background: transparent; border: none; font-size: 1.5rem; cursor: pointer; display: flex; align-items: center; justify-content: center; }
.text-red { color: #ef4444; }
.btn-outline { background: transparent; border: 1px solid #94a3b8; color: #475569; padding: 6px 12px; border-radius: 6px; cursor: pointer; font-size: 0.85rem; }
.table-responsive { overflow-x: auto; }
.glass-table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
.glass-table th, .glass-table td { padding: 12px; border-bottom: 1px solid #e2e8f0; vertical-align: middle; }
.glass-table th { background: #f8fafc; text-align: left; font-weight: 600; color: #475569; }
.glass-table th:nth-child(1), .glass-table td:nth-child(1) { width: 64px; }
.glass-table th:nth-child(3), .glass-table th:nth-child(4), .glass-table td:nth-child(3), .glass-table td:nth-child(4) { width: 96px; }
.glass-table th:nth-child(5), .glass-table td:nth-child(5) { min-width: 220px; }
.text-center { text-align: center !important; }
.print-signature-grid div { display: grid; place-items: center; min-height: 88px; border: 1px dashed #cbd5e1; border-radius: 10px; background: #f8fafc; color: #475569; font-weight: 700; text-align: center; }
.form-actions { display: flex; justify-content: flex-end; gap: 15px; }
.btn-primary, .btn-secondary { padding: 12px 24px; border-radius: 8px; font-size: 1rem; font-weight: 600; cursor: pointer; display: flex; align-items: center; gap: 8px; border: none; }
.btn-primary { background: #0ea5e9; color: white; }
.btn-primary:disabled { opacity: 0.7; cursor: not-allowed; }
.btn-secondary { background: #f1f5f9; color: #475569; border: 1px solid #cbd5e1; }
.spinner { width: 20px; height: 20px; border: 3px solid rgba(255,255,255,0.3); border-top-color: #fff; border-radius: 50%; animation: spin 0.8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 768px) {
  .form-container { padding-bottom: max(320px, env(safe-area-inset-bottom)); }
  .form-row, .signature-grid, .print-signature-grid { grid-template-columns: minmax(0, 1fr); width: 100%; max-width: 100%; }
  .glass-card { overflow: hidden; padding: 18px; }
  input[type='date'].glass-input { width: 100%; max-width: 100%; min-width: 0; -webkit-appearance: none; appearance: none; }
  .thanh-phan-item { grid-template-columns: 36px minmax(0, 1fr) 38px; gap: 8px; }
  .thanh-phan-item .glass-input { grid-column: 2 / 3; width: 100%; min-width: 0; padding: 9px 10px; font-size: 0.86rem; }
  .thanh-phan-item .btn-icon { grid-column: 3; grid-row: 1; width: 38px; height: 38px; border-radius: 10px; background: #fff1f2; }
  .table-responsive { overflow-x: auto; -webkit-overflow-scrolling: touch; padding-bottom: 8px; }
  .glass-table { min-width: 820px; }
  .note-input { min-width: 180px; min-height: 72px; font-size: 0.95rem; }
  .form-actions { flex-direction: column; align-items: stretch; }
  .glass-input:focus, .glass-input-sm:focus { border-color: #38bdf8; box-shadow: 0 0 0 3px rgba(14, 165, 233, 0.18); outline: none; }
}
</style>
