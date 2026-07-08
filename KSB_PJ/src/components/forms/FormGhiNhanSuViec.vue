<script setup>
import { onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import AppToast from '@/components/AppToast.vue'
import { clearFormDraft, loadFormDraft, saveFormDraft, scrollFocusedFieldIntoView } from '@/utils/formDraftStore'
import { saveReport } from '@/utils/reportStore'

const router = useRouter()
const DRAFT_KEY = 'bb_ghinhan_suviec'

const form = ref({
  soBienBan: '',
  ngayKiemTra: new Date().toISOString().split('T')[0],
  thoiGian: '',
  diaDiem: '',
  daiDienDonViCungCap: '',
  thanhVienKiemTra: '',
  noiDungViPham: '',
  xacDinhNguyenNhan: '',
  bienPhapKhacPhuc: ''
})

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

const makeDetail = (mucSo, phanNhom, noiDung, ghiChu) => ({
  mucSo,
  phanNhom,
  noiDung,
  dat: null,
  ghiChu: ghiChu || ''
})

const restoreDraft = () => {
  const draft = loadFormDraft(DRAFT_KEY)
  if (draft?.form) {
    form.value = {
      ...form.value,
      ...draft.form
    }
  }
}

const cancelForm = () => {
  clearFormDraft(DRAFT_KEY)
  router.push('/employee')
}

const submitForm = async () => {
  isSubmitting.value = true

  try {
    await saveReport({
      soBienBan: form.value.soBienBan || `BB-DD-${Date.now().toString().slice(-4)}`,
      loaiBienBan: 'GhiNhanSuViec',
      ngayKiemTra: form.value.ngayKiemTra,
      thanhPhans: [
        {
          stt: 1,
          hoTen: form.value.daiDienDonViCungCap || 'Đại diện đơn vị cung cấp',
          chucVu: 'Đại diện đơn vị cung cấp'
        },
        {
          stt: 2,
          hoTen: form.value.thanhVienKiemTra || 'Thành viên kiểm tra VSATTP',
          chucVu: 'Thành viên kiểm tra VSATTP'
        }
      ],
      chiTiets: [
        makeDetail(1, 'I. Hành chính', 'Thời gian', form.value.thoiGian),
        makeDetail(2, 'I. Hành chính', 'Địa điểm', form.value.diaDiem),
        makeDetail(3, 'I. Hành chính - Thành phần', 'Đại diện đơn vị cung cấp', form.value.daiDienDonViCungCap),
        makeDetail(4, 'I. Hành chính - Thành phần', 'Thành viên kiểm tra VSATTP', form.value.thanhVienKiemTra),
        makeDetail(5, 'II. Nội dung', 'Nội dung vi phạm', form.value.noiDungViPham),
        makeDetail(6, 'II. Nội dung', 'Xác định nguyên nhân', form.value.xacDinhNguyenNhan),
        makeDetail(7, 'II. Nội dung', 'Biện pháp khắc phục, phòng ngừa', form.value.bienPhapKhacPhuc)
      ],
      gopYKhoaDinhDuong: form.value.xacDinhNguyenNhan,
      yKienBPCB: form.value.bienPhapKhacPhuc,
      chuKys: []
    })

    clearFormDraft(DRAFT_KEY)
    showToast('Đã gửi biên bản ghi nhận sự việc lên admin thành công!')
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
  form,
  () => {
    saveFormDraft(DRAFT_KEY, { form: form.value })
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
      <h2>Biên bản ghi nhận sự việc</h2>
      <p class="subtitle">Khoa Dinh Dưỡng - BV Hoàn Mỹ Đồng Nai</p>
    </div>

    <form @submit.prevent="submitForm">
      <div class="glass-card section-card">
        <h3>Thông tin chung</h3>
        <div class="form-row">
          <div class="form-group">
            <label>Số biên bản</label>
            <input v-model="form.soBienBan" type="text" class="glass-input" placeholder="VD: 01/BB-DD" />
          </div>
          <div class="form-group">
            <label>Ngày ghi nhận</label>
            <input v-model="form.ngayKiemTra" type="date" class="glass-input" required />
          </div>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>Thời gian</label>
            <input v-model="form.thoiGian" type="text" class="glass-input" placeholder="VD: 09:00" required />
          </div>
          <div class="form-group">
            <label>Địa điểm</label>
            <input v-model="form.diaDiem" type="text" class="glass-input" placeholder="Nhập địa điểm ghi nhận" required />
          </div>
        </div>
      </div>

      <div class="glass-card section-card">
        <h3>Thành phần</h3>
        <div class="signature-grid">
          <div class="form-group">
            <label>Đại diện đơn vị cung cấp</label>
            <input
              v-model="form.daiDienDonViCungCap"
              type="text"
              class="glass-input"
              placeholder="Họ tên / đơn vị"
              @focus="scrollFocusedFieldIntoView"
            />
          </div>
          <div class="form-group">
            <label>Thành viên kiểm tra VSATTP</label>
            <input
              v-model="form.thanhVienKiemTra"
              type="text"
              class="glass-input"
              placeholder="Họ tên người kiểm tra"
              @focus="scrollFocusedFieldIntoView"
            />
          </div>
        </div>
      </div>

      <div class="glass-card section-card">
        <h3>Nội dung sự việc</h3>
        <div class="form-group">
          <label>Nội dung vi phạm</label>
          <textarea
            v-model="form.noiDungViPham"
            rows="8"
            class="glass-input note-input tall"
            placeholder="Nhập nội dung vi phạm..."
            required
            @focus="scrollFocusedFieldIntoView"
          ></textarea>
        </div>

        <div class="signature-grid">
          <div class="form-group">
            <label>Xác định nguyên nhân</label>
            <textarea
              v-model="form.xacDinhNguyenNhan"
              rows="5"
              class="glass-input note-input"
              placeholder="Nhập nguyên nhân..."
              @focus="scrollFocusedFieldIntoView"
            ></textarea>
          </div>
          <div class="form-group">
            <label>Biện pháp khắc phục, phòng ngừa</label>
            <textarea
              v-model="form.bienPhapKhacPhuc"
              rows="5"
              class="glass-input note-input"
              placeholder="Nhập biện pháp khắc phục..."
              @focus="scrollFocusedFieldIntoView"
            ></textarea>
          </div>
        </div>
      </div>

      <div class="glass-card section-card">
        <h3>Vị trí ký khi in biên bản</h3>
        <div class="print-signature-grid">
          <div>Đại diện đơn vị cung cấp</div>
          <div>Người kiểm tra</div>
          <div>Lãnh đạo Khoa Dinh Dưỡng</div>
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
.header-card { text-align: center; background: linear-gradient(135deg, #fdf2f8, #ffe4e6); border-bottom: 3px solid #e11d48; }
.header-card h2 { margin: 0 0 10px; font-size: 1.8rem; color: #0f172a; }
.subtitle { color: #475569; font-weight: 600; letter-spacing: 1px; text-transform: uppercase; }
.section-card h3 { margin: 0 0 20px; padding-bottom: 10px; border-bottom: 1px solid #e2e8f0; color: #0f172a; }
.form-row, .signature-grid, .print-signature-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; min-width: 0; }
.print-signature-grid { grid-template-columns: repeat(3, minmax(0, 1fr)); }
.form-group { display: flex; flex-direction: column; gap: 8px; min-width: 0; margin-bottom: 16px; }
.form-group:last-child { margin-bottom: 0; }
.form-group label { font-size: 0.9rem; font-weight: 600; color: #475569; }
.glass-input { width: 100%; max-width: 100%; box-sizing: border-box; background: #f8fafc; border: 1px solid #cbd5e1; border-radius: 8px; color: #1e293b; padding: 12px 16px; font-family: inherit; font-size: 0.95rem; }
.note-input { display: block; min-height: 112px; line-height: 1.5; resize: vertical; white-space: pre-wrap; overflow-wrap: anywhere; }
.note-input.tall { min-height: 180px; }
.print-signature-grid div { display: grid; place-items: center; min-height: 88px; border: 1px dashed #cbd5e1; border-radius: 10px; background: #f8fafc; color: #475569; font-weight: 700; text-align: center; }
.form-actions { display: flex; justify-content: flex-end; gap: 15px; }
.btn-primary, .btn-secondary { padding: 12px 24px; border-radius: 8px; font-size: 1rem; font-weight: 600; cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 8px; border: none; }
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
  .form-actions { flex-direction: column; align-items: stretch; }
  .glass-input:focus { border-color: #38bdf8; box-shadow: 0 0 0 3px rgba(14, 165, 233, 0.18); outline: none; }
}
</style>
