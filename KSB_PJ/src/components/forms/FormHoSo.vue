<script setup>
import { onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import AppToast from '@/components/AppToast.vue'
import { clearFormDraft, loadFormDraft, saveFormDraft, scrollFocusedFieldIntoView } from '@/utils/formDraftStore'
import { saveReport } from '@/utils/reportStore'
import { applyReportTemplate } from '@/utils/templateStore'

const router = useRouter()
const DRAFT_KEY = 'bb_hoso'

const form = ref({
  ngayKiemTra: new Date().toISOString().split('T')[0],
  thanhPhans: [
    { stt: 1, hoTen: '', chucVu: '' }
  ],
  yKienKhoaDinhDuong: '',
  yKienBoPhanCheBien: ''
})

const items = ref([
  { mucSo: 1, noiDung: 'Giấy phép kinh doanh', dat: null, ghiChu: '' },
  { mucSo: 2, noiDung: 'Giấy chứng nhận cơ sở đủ điều kiện An toàn thực phẩm', dat: null, ghiChu: '' },
  { mucSo: 3, noiDung: 'Giấy xác nhận tập huấn kiến thức về ATVSTP', dat: null, ghiChu: '' },
  { mucSo: 4, noiDung: 'Giấy xác nhận đủ sức khỏe của nhân sự trực tiếp', dat: null, ghiChu: '' },
  { mucSo: 5, noiDung: 'Hợp đồng mua bán', dat: null, ghiChu: '' },
  { mucSo: 6, noiDung: 'Hóa đơn mua hàng', dat: null, ghiChu: '' },
  { mucSo: 7, noiDung: 'Danh sách nhân viên đang làm việc tại bếp', dat: null, ghiChu: '' },
  { mucSo: 8, noiDung: 'Bảng mô tả công việc cho từng vị trí', dat: null, ghiChu: '' },
  { mucSo: 9, noiDung: 'Sổ kiểm thực 03 bước, sổ lưu mẫu thức ăn', dat: null, ghiChu: '' },
  { mucSo: 10, noiDung: 'Bảng giá niêm yết', dat: null, ghiChu: '' },
  { mucSo: 11, noiDung: 'Thực đơn/Kế hoạch sản xuất', dat: null, ghiChu: '' },
  { mucSo: 12, noiDung: 'Bảng định lượng suất ăn', dat: null, ghiChu: '' },
  { mucSo: 13, noiDung: 'Phân công vệ sinh', dat: null, ghiChu: '' },
  { mucSo: 14, noiDung: 'Kế hoạch diệt côn trùng', dat: null, ghiChu: '' },
  { mucSo: 15, noiDung: 'Quy trình hướng dẫn cho từng khu vực', dat: null, ghiChu: '' },
  { mucSo: 16, noiDung: 'Kế hoạch đào tạo, tập huấn cho nhân viên', dat: null, ghiChu: '' }
])

applyReportTemplate('HoSo', items.value)

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
  if (Array.isArray(draft.items)) items.value = draft.items
}

const cancelForm = () => {
  clearFormDraft(DRAFT_KEY)
  router.push('/employee')
}

const submitForm = async () => {
  isSubmitting.value = true

  try {
    await saveReport({
      soBienBan: `BB-HS-${Date.now().toString().slice(-4)}`,
      loaiBienBan: 'HoSo',
      ngayKiemTra: form.value.ngayKiemTra,
      thanhPhans: form.value.thanhPhans,
      chiTiets: items.value.map(item => ({
        mucSo: item.mucSo,
        phanNhom: '',
        noiDung: item.noiDung,
        dat: item.dat,
        ghiChu: item.ghiChu
      })),
      yKienKhoaDinhDuong: form.value.yKienKhoaDinhDuong,
      yKienBoPhanCheBien: form.value.yKienBoPhanCheBien,
      chuKys: []
    })

    clearFormDraft(DRAFT_KEY)
    showToast('Đã gửi biên bản hồ sơ sổ sách lên admin thành công!')
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
      <h2>Biên bản kiểm tra Hồ sơ, sổ sách, các chứng từ</h2>
      <p class="subtitle">Tại bộ phận chế biến và cung cấp suất ăn</p>
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
              <tr v-for="item in items" :key="item.mucSo">
                <td class="text-center">{{ item.mucSo }}</td>
                <td>{{ item.noiDung }}</td>
                <td class="text-center"><input v-model="item.dat" type="radio" :name="`hoso_${item.mucSo}`" :value="true" /></td>
                <td class="text-center"><input v-model="item.dat" type="radio" :name="`hoso_${item.mucSo}`" :value="false" /></td>
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
            <label>Khoa Dinh dưỡng</label>
            <textarea v-model="form.yKienKhoaDinhDuong" rows="4" class="glass-input" @focus="scrollFocusedFieldIntoView"></textarea>
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
  </div>
</template>

<style scoped>
.form-container { display: flex; flex-direction: column; gap: 20px; max-width: 1200px; margin: 0 auto; padding-bottom: 50px; }
.form-container form { display: flex; flex-direction: column; gap: 20px; }
.glass-card { background: #ffffff; border: 1px solid #e2e8f0; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05), 0 2px 4px -1px rgba(0,0,0,0.03); border-radius: 12px; padding: 24px; color: #334155; }
.header-card { text-align: center; background: linear-gradient(135deg, #ecfeff, #e0f2fe); border-bottom: 3px solid #0891b2; }
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
.stt-badge { width: 36px; height: 36px; display: flex; align-items: center; justify-content: center; background: #ecfeff; color: #0f766e; border-radius: 50%; font-weight: 700; flex-shrink: 0; }
.flex-1 { flex: 1; }
.btn-icon { background: transparent; border: none; font-size: 1.5rem; cursor: pointer; display: flex; }
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
  .thanh-phan-item {
    display: grid;
    grid-template-columns: 34px minmax(0, 1fr) minmax(0, 1fr) 38px;
    gap: 8px;
    align-items: center;
    width: 100%;
    max-width: 100%;
    flex-direction: initial;
  }
  .thanh-phan-item .glass-input {
    width: 100%;
    max-width: 100%;
    min-width: 0;
    padding: 9px 10px;
    font-size: 0.86rem;
  }
  .thanh-phan-item .btn-icon {
    width: 38px;
    height: 38px;
    align-items: center;
    justify-content: center;
    border-radius: 10px;
    background: #fff1f2;
  }
  .stt-badge {
    width: 34px;
    height: 34px;
  }
  .flex-1 { min-width: 0; }
  .table-responsive { overflow-x: auto; -webkit-overflow-scrolling: touch; padding-bottom: 8px; }
  .glass-table { min-width: 760px; }
  .note-input { min-width: 180px; min-height: 72px; font-size: 0.95rem; }
  .glass-input:focus, .glass-input-sm:focus { border-color: #38bdf8; box-shadow: 0 0 0 3px rgba(14, 165, 233, 0.18); outline: none; }
}
</style>
