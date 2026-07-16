<script setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import AppToast from '@/components/AppToast.vue'
import { clearFormDraft, loadFormDraft, saveFormDraft, scrollFocusedFieldIntoView } from '@/utils/formDraftStore'
import { saveReport } from '@/utils/reportStore'
import { applyReportTemplate } from '@/utils/templateStore'

const router = useRouter()
const DRAFT_KEY = 'bb_suatan'

const form = ref({
  ngayKiemTra: new Date().toISOString().split('T')[0],
  thanhPhans: [
    { stt: 1, hoTen: '', chucVu: '' }
  ],
  buaAn: [],
  buaAnOngThong: [],
  thucDonThayDoi: null,
  yKienKhoaDinhDuong: '',
  yKienBoPhanCheBien: ''
})

const mealOptions = ['Sáng', 'Trưa', 'Xế', 'Chiều', 'Tối']
const tubeMealOptions = ['Sáng', 'Trưa', 'Xế', 'Chiều']

const makeMealRow = (mucSo, noiDung = '', dishCount = 1) => ({
  mucSo,
  noiDung,
  dishCount,
  dishes: Array.from({ length: dishCount }, () => ''),
  ghiChuNoiDung: '',
  cheDo1KhoiLuong: '',
  cheDo1Dat: null,
  cheDo2KhoiLuong: '',
  cheDo2Dat: null,
  ghiChu: ''
})

const makeRequirementRow = (mucSo, noiDung, moTa = '', phanNhom = '') => ({
  mucSo,
  noiDung,
  moTa,
  phanNhom,
  dat: null,
  ghiChu: ''
})

const formatRequirementDescription = items => items.map(item => `- ${item}`).join('\n')

const oralRequirementDefinitions = [
  {
    title: 'Món cơm',
    items: [
      'Gạo đảm bảo chất lượng còn hạn sử dụng.',
      'Tỷ lệ gạo - nước, số lượng gạo/khay được định lượng với tỷ lệ phù hợp.',
      'Cơm thành phẩm chín đều, tơi xốp, không bị khô hay nhão, cháy khét hay có mùi lạ.'
    ]
  },
  {
    title: 'Món cháo',
    items: [
      'Gạo đảm bảo chất lượng.',
      'Cháo chín đều, không bị vón cục, cháo có màu trắng đặc trưng, có mùi vị thơm ngon, có độ đặc/lỏng phù hợp yêu cầu của chế độ ăn.',
      'Nguyên liệu trong cháo phải được nấu chín mềm/xay theo yêu cầu.'
    ]
  },
  {
    title: 'Món nước',
    items: [
      'Nước dùng nấu ngọt từ xương, rau củ, nước trong, mùi đặc trưng, vị phù hợp bệnh lý.',
      'Chất lượng nguyên liệu ăn kèm: củ chín mềm, giữ được màu sắc đặc trưng, rau ăn kèm sạch, tươi.',
      'Hủ tiếu, phở, miến, nui... trụng vừa tới, không có nhão cứng.',
      'Bảo quản: nước dùng sau khi nấu nếu chưa phục vụ ngay phải được bảo quản trong thiết bị hâm nóng; các loại bánh phở, hủ tiếu, nui phải được bao bọc kín.'
    ]
  },
  {
    title: 'Món canh',
    items: [
      'Nước canh trong, rau củ giữ được màu đặc trưng, rau lá chín tới, củ chín mềm; thịt cá chín mềm không dai, không nát.'
    ]
  },
  {
    title: 'Món xào',
    items: [
      'Rau củ giữ được màu đặc trưng, rau chín tới, củ chín mềm; thịt cá chín mềm không dai, không nát.'
    ]
  },
  {
    title: 'Món kho, rim',
    items: [
      'Thực phẩm thấm gia vị, chín đều, mềm, vị mặn, màu vàng nâu.'
    ]
  },
  {
    title: 'Món chiên',
    items: [
      'Dầu không sử dụng lại, dùng chiên 1 lần.',
      'Món ăn có màu vàng đều, không bị cháy xém, giòn xốp, ráo dầu và chín thấm, thơm ngon, vừa ăn.'
    ]
  }
]

const rows = ref([
  makeMealRow(1, 'Cơm', 0),
  makeMealRow(2, 'Món mặn', 4),
  makeMealRow(3, 'Món chay', 2),
  makeMealRow(4, 'Món xào/luộc', 1),
  makeMealRow(5, 'Món canh', 1),
  makeMealRow(6, 'Tráng miệng', 1),
  makeMealRow(7, 'Cháo mặn', 0),
  makeMealRow(8, 'Cháo chay', 0),
  makeMealRow(9, 'Món nước', 2)
])

applyReportTemplate('SuatAnNguoiBenh', rows.value)

const tubeRows = ref(Array.from({ length: 6 }, (_, index) => makeMealRow(index + 1, `Bữa ăn ${index + 1}`, 1)))

const oralRequirementRows = ref(
  oralRequirementDefinitions.map((item, index) =>
    makeRequirementRow(
      index + 1,
      item.title,
      formatRequirementDescription(item.items),
      'I. Suất ăn đường miệng - Các yêu cầu riêng'
    )
  )
)

const tubeRequirementRows = ref([
  makeRequirementRow(
    1,
    'Món súp xay ăn qua ống thông',
    formatRequirementDescription([
      'Món ăn lỏng, đồng nhất, ít lợn cợn, đảm bảo lưu thông qua ống thông dễ dàng.',
      'Mùi thơm, không tanh hôi.'
    ]),
    'II. Suất ăn qua ống thông - Các yêu cầu riêng'
  )
])

const isSubmitting = ref(false)
const toast = ref({ visible: false, message: '' })

let toastTimer = null

// Progress tracking: count all radio-button fields across all 4 tables
const completedCount = computed(() => {
  let count = 0
  // Oral meal rows: cheDo1Dat
  count += rows.value.filter(r => r.cheDo1Dat !== null).length
  // Oral requirement rows: dat
  count += oralRequirementRows.value.filter(r => r.dat !== null).length
  // Tube requirement rows: dat
  count += tubeRequirementRows.value.filter(r => r.dat !== null).length
  return count
})
const totalCount = computed(() => {
  return rows.value.length + oralRequirementRows.value.length + tubeRequirementRows.value.length
})
const progressPercent = computed(() => Math.round((completedCount.value / totalCount.value) * 100))

const scrollToFirstUnchecked = () => {
  // Check oral meal rows first
  const uncheckedOralMeal = rows.value.find(r => r.cheDo1Dat === null)
  if (uncheckedOralMeal) {
    const el = document.getElementById(`oral-row-${uncheckedOralMeal.mucSo}`)
    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'center' })
      el.classList.add('flash-highlight')
      setTimeout(() => el.classList.remove('flash-highlight'), 1800)
    }
    return
  }
  // Check oral requirement rows
  const uncheckedOralReq = oralRequirementRows.value.find(r => r.dat === null)
  if (uncheckedOralReq) {
    const el = document.getElementById(`oral-req-row-${uncheckedOralReq.mucSo}`)
    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'center' })
      el.classList.add('flash-highlight')
      setTimeout(() => el.classList.remove('flash-highlight'), 1800)
    }
    return
  }
  // Check tube requirement rows
  const uncheckedTubeReq = tubeRequirementRows.value.find(r => r.dat === null)
  if (uncheckedTubeReq) {
    const el = document.getElementById(`tube-req-row-${uncheckedTubeReq.mucSo}`)
    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'center' })
      el.classList.add('flash-highlight')
      setTimeout(() => el.classList.remove('flash-highlight'), 1800)
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

const renumberRows = items => {
  items.forEach((item, index) => {
    item.mucSo = index + 1
  })
}

const addTubeRow = () => {
  tubeRows.value.push(makeMealRow(tubeRows.value.length + 1))
}

const removeTubeRow = index => {
  if (tubeRows.value.length <= 1) return
  tubeRows.value.splice(index, 1)
  renumberRows(tubeRows.value)
}

const normalizeRequirementDraft = draft => {
  if (Array.isArray(draft.oralRequirementRows) || Array.isArray(draft.tubeRequirementRows)) {
    if (Array.isArray(draft.oralRequirementRows)) oralRequirementRows.value = draft.oralRequirementRows
    if (Array.isArray(draft.tubeRequirementRows)) tubeRequirementRows.value = draft.tubeRequirementRows
    return
  }

  if (Array.isArray(draft.requirementRows)) {
    const oralRows = draft.requirementRows.filter(item => String(item.phanNhom || '').startsWith('I.'))
    const tubeReqRows = draft.requirementRows.filter(item => String(item.phanNhom || '').startsWith('II.'))

    if (oralRows.length) oralRequirementRows.value = oralRows
    if (tubeReqRows.length) {
      tubeRequirementRows.value = tubeReqRows
    } else if (draft.requirementRows.length) {
      tubeRequirementRows.value = draft.requirementRows
    }
  }
}

const cancelForm = () => {
  clearFormDraft(DRAFT_KEY)
  router.push('/employee')
}

const restoreDraft = () => {
  const draft = loadFormDraft(DRAFT_KEY)
  if (!draft) return
  if (draft.form) {
    const hasTypedData = draft.form.thanhPhans?.some(tp => tp.hoTen?.trim() !== '')
    if (!hasTypedData) {
      draft.form.thanhPhans = [{ stt: 1, hoTen: '', chucVu: '' }]
    }
    form.value = draft.form
  }
  if (Array.isArray(draft.rows)) {
    rows.value = rows.value.map(defaultRow => {
      const draftRow = draft.rows.find(r => r.mucSo === defaultRow.mucSo)
      if (draftRow) {
        const existingDishes = draftRow.dishes || []
        const dishes = Array.from({ length: defaultRow.dishCount }, (_, idx) => existingDishes[idx] || '')
        return {
          ...draftRow,
          dishCount: defaultRow.dishCount,
          dishes
        }
      }
      return defaultRow
    })
  }
  if (Array.isArray(draft.tubeRows)) tubeRows.value = draft.tubeRows
  normalizeRequirementDraft(draft)
}

const submitForm = async () => {
  if (completedCount.value < totalCount.value) {
    showToast('Vui lòng hoàn thành tất cả tiêu chí đánh giá!')
    scrollToFirstUnchecked()
    return
  }

  isSubmitting.value = true

  // Set ghiChuNoiDung for oral meal rows
  rows.value.forEach(row => {
    if (row.dishes && row.dishes.length) {
      row.ghiChuNoiDung = row.dishes.filter(d => d.trim() !== '').join(' - ')
    } else {
      row.ghiChuNoiDung = ''
    }
  })

  try {
    await saveReport({
      soBienBan: `BB-SA-${Date.now().toString().slice(-4)}`,
      loaiBienBan: 'SuatAnNguoiBenh',
      ngayKiemTra: form.value.ngayKiemTra,
      thanhPhans: form.value.thanhPhans,
      buaAn: form.value.buaAn,
      buaAnOngThong: form.value.buaAnOngThong,
      thucDonThayDoi: form.value.thucDonThayDoi,
      chiTiets: rows.value,
      chiTietOngThong: tubeRows.value,
      yeuCauRieng: [...oralRequirementRows.value, ...tubeRequirementRows.value],
      yKienKhoaDinhDuong: form.value.yKienKhoaDinhDuong,
      yKienBoPhanCheBien: form.value.yKienBoPhanCheBien
    })

    clearFormDraft(DRAFT_KEY)
    showToast('Đã gửi biên bản suất ăn người bệnh lên admin thành công!')
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
  [form, rows, tubeRows, oralRequirementRows, tubeRequirementRows],
  () => {
    saveFormDraft(DRAFT_KEY, {
      form: form.value,
      rows: rows.value,
      tubeRows: tubeRows.value,
      oralRequirementRows: oralRequirementRows.value,
      tubeRequirementRows: tubeRequirementRows.value
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
      <h2>Biên bản kiểm tra Suất ăn người bệnh</h2>
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
        <h3>I. Suất ăn đường miệng</h3>
        <div class="option-grid">
          <div class="form-group">
            <label>Bữa ăn</label>
            <div class="checkbox-row">
              <label v-for="option in mealOptions" :key="option" class="checkbox-item">
                <input v-model="form.buaAn" type="checkbox" :value="option" />
                <span>{{ option }}</span>
              </label>
            </div>
          </div>

          <div class="form-group">
            <label>Thực đơn hằng ngày</label>
            <div class="checkbox-row">
              <label class="checkbox-item"><input v-model="form.thucDonThayDoi" type="radio" :value="true" /> <span>Thay đổi</span></label>
              <label class="checkbox-item"><input v-model="form.thucDonThayDoi" type="radio" :value="false" /> <span>Không thay đổi</span></label>
            </div>
          </div>
        </div>

        <h4 class="table-title">Định lượng phần ăn</h4>
        <div class="table-responsive">
          <table class="glass-table meal-table">
            <thead>
              <tr>
                <th>TT</th>
                <th>Nội dung</th>
                <th>CĐ1 khối lượng (g)</th>
                <th>CĐ1 Đạt</th>
                <th>CĐ1 Không đạt</th>
                <th>CĐ2 khối lượng (g)</th>
                <th>CĐ2 Đạt</th>
                <th>CĐ2 Không đạt</th>
                <th>Ghi chú</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in rows" :key="row.mucSo" :id="'oral-row-' + row.mucSo">
                <!-- TT -->
                <td class="text-center">{{ row.mucSo }}</td>
                <!-- Nội dung + dotted lines for dish names -->
                <td class="noi-dung-cell">
                  <strong>{{ row.noiDung }}{{ row.dishCount > 0 ? ':' : '' }}</strong>
                  <div v-if="row.dishCount > 0" class="dotted-lines">
                    <div v-for="(dish, dIdx) in row.dishes" :key="dIdx" class="dotted-line-item">
                      <input
                        v-model="row.dishes[dIdx]"
                        type="text"
                        class="dotted-line-input"
                        :placeholder="' '"
                        @focus="scrollFocusedFieldIntoView"
                      />
                    </div>
                  </div>
                </td>
                <td><input v-model="row.cheDo1KhoiLuong" type="text" class="glass-input-sm" placeholder="g" /></td>
                <td class="text-center"><input v-model="row.cheDo1Dat" type="radio" :name="`cd1_${row.mucSo}`" :value="true" /></td>
                <td class="text-center"><input v-model="row.cheDo1Dat" type="radio" :name="`cd1_${row.mucSo}`" :value="false" /></td>
                <td><input v-model="row.cheDo2KhoiLuong" type="text" class="glass-input-sm" placeholder="g" /></td>
                <td class="text-center"><input v-model="row.cheDo2Dat" type="radio" :name="`cd2_${row.mucSo}`" :value="true" /></td>
                <td class="text-center"><input v-model="row.cheDo2Dat" type="radio" :name="`cd2_${row.mucSo}`" :value="false" /></td>
                <td>
                  <textarea v-model="row.ghiChu" rows="1" class="glass-input-sm note-input" placeholder="Ghi chú..." @focus="scrollFocusedFieldIntoView"></textarea>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <h4 class="table-title">Các yêu cầu riêng</h4>
        <div class="table-responsive requirement-table-wrap">
          <table class="glass-table requirement-table">
            <thead>
              <tr>
                <th>TT</th>
                <th>Nội dung</th>
                <th>Đạt</th>
                <th>Không đạt</th>
                <th>Ghi chú</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in oralRequirementRows" :key="`oral-req-${row.mucSo}`" :id="'oral-req-row-' + row.mucSo">
                <td class="text-center">{{ row.mucSo }}</td>
                <td>
                  <strong>{{ row.noiDung }}:</strong>
                  <div class="muted pre-line">{{ row.moTa }}</div>
                </td>
                <td class="text-center"><input v-model="row.dat" type="radio" :name="`oral_req_${row.mucSo}`" :value="true" /></td>
                <td class="text-center"><input v-model="row.dat" type="radio" :name="`oral_req_${row.mucSo}`" :value="false" /></td>
                <td>
                  <textarea v-model="row.ghiChu" rows="2" class="glass-input-sm note-input" placeholder="Ghi chú..." @focus="scrollFocusedFieldIntoView"></textarea>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div class="glass-card section-card">
        <h3>II. Suất ăn qua ống thông</h3>
        <div class="form-group">
          <label>Bữa ăn</label>
          <div class="checkbox-row">
            <label v-for="option in tubeMealOptions" :key="option" class="checkbox-item">
              <input v-model="form.buaAnOngThong" type="checkbox" :value="option" />
              <span>{{ option }}</span>
            </label>
          </div>
        </div>

        <div class="section-topline table-topline">
          <h4 class="table-title">Định lượng phần ăn</h4>
          <button type="button" class="btn-outline" @click="addTubeRow">+ Thêm dòng</button>
        </div>
        <div class="table-responsive">
          <table class="glass-table meal-table tube-table">
            <thead>
              <tr>
                <th>TT</th>
                <th>Nội dung</th>
                <th>Chế độ ăn 1 - Khối lượng (g)</th>
                <th>Đạt</th>
                <th>Không đạt</th>
                <th>Chế độ ăn 2 - Khối lượng (g)</th>
                <th>Đạt</th>
                <th>Không đạt</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(row, index) in tubeRows" :key="`tube-${row.mucSo}`">
                <td class="text-center">{{ row.mucSo }}</td>
                <td><input v-model="row.noiDung" type="text" class="glass-input-sm" placeholder="Nhập nội dung..." /></td>
                <td><input v-model="row.cheDo1KhoiLuong" type="text" class="glass-input-sm" placeholder="g" /></td>
                <td class="text-center"><input v-model="row.cheDo1Dat" type="radio" :name="`tube_cd1_${row.mucSo}`" :value="true" /></td>
                <td class="text-center"><input v-model="row.cheDo1Dat" type="radio" :name="`tube_cd1_${row.mucSo}`" :value="false" /></td>
                <td><input v-model="row.cheDo2KhoiLuong" type="text" class="glass-input-sm" placeholder="g" /></td>
                <td class="text-center"><input v-model="row.cheDo2Dat" type="radio" :name="`tube_cd2_${row.mucSo}`" :value="true" /></td>
                <td class="text-center"><input v-model="row.cheDo2Dat" type="radio" :name="`tube_cd2_${row.mucSo}`" :value="false" /></td>
                <td class="text-center">
                  <button v-if="tubeRows.length > 1" type="button" class="btn-icon text-red remove-row-btn" @click="removeTubeRow(index)">
                    <ion-icon name="trash-outline"></ion-icon>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <h4 class="table-title">Các yêu cầu riêng</h4>
        <div class="table-responsive requirement-table-wrap">
          <table class="glass-table requirement-table">
            <thead>
              <tr>
                <th>TT</th>
                <th>Nội dung</th>
                <th>Đạt</th>
                <th>Không đạt</th>
                <th>Ghi chú</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in tubeRequirementRows" :key="`tube-req-${row.mucSo}`" :id="'tube-req-row-' + row.mucSo">
                <td class="text-center">{{ row.mucSo }}</td>
                <td>
                  <strong>{{ row.noiDung }}:</strong>
                  <div class="muted pre-line">{{ row.moTa }}</div>
                </td>
                <td class="text-center"><input v-model="row.dat" type="radio" :name="`tube_req_${row.mucSo}`" :value="true" /></td>
                <td class="text-center"><input v-model="row.dat" type="radio" :name="`tube_req_${row.mucSo}`" :value="false" /></td>
                <td>
                  <textarea v-model="row.ghiChu" rows="2" class="glass-input-sm note-input" placeholder="Ghi chú..." @focus="scrollFocusedFieldIntoView"></textarea>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div class="glass-card section-card">
        <h3>III. Góp ý, nhắc nhở của Khoa Dinh dưỡng</h3>
        <textarea v-model="form.yKienKhoaDinhDuong" rows="4" class="glass-input" @focus="scrollFocusedFieldIntoView"></textarea>
      </div>

      <div class="glass-card section-card">
        <h3>IV. Ý kiến của BPCB & CCSA</h3>
        <textarea v-model="form.yKienBoPhanCheBien" rows="4" class="glass-input" @focus="scrollFocusedFieldIntoView"></textarea>
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
        <div class="progress-percent-circle">
          <span>{{ progressPercent }}%</span>
        </div>
        <div class="progress-track">
          <div class="progress-fill" :style="{ '--progress-val': progressPercent + '%' }"></div>
        </div>
        <div class="progress-text-fraction">
          <strong>{{ completedCount }}/{{ totalCount }}</strong>
          <span>tiêu chí</span>
        </div>
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
</template>

<style scoped>
.form-container { display: flex; flex-direction: column; gap: 20px; max-width: 1200px; margin: 0 auto; padding-bottom: 120px !important; }
.form-container form { display: flex; flex-direction: column; gap: 20px; }
.glass-card { background: #ffffff; border: 1px solid #e2e8f0; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05), 0 2px 4px -1px rgba(0,0,0,0.03); border-radius: 12px; padding: 24px; color: #334155; }
.header-card { text-align: center; background: linear-gradient(135deg, #fff7ed, #ffedd5); border-bottom: 3px solid #ea580c; }
.header-card h2 { margin: 0 0 10px; font-size: 1.8rem; color: #0f172a; }
.subtitle { color: #475569; font-weight: 600; letter-spacing: 1px; text-transform: uppercase; }
.section-card h3 { margin: 0 0 20px; padding-bottom: 10px; border-bottom: 1px solid #e2e8f0; color: #0f172a; }
.thanh-phan-list { margin-top: 24px; padding-top: 20px; border-top: 1px solid #e2e8f0; }
.section-topline { display: flex; justify-content: space-between; align-items: center; gap: 12px; margin-bottom: 14px; }
.table-topline { margin-top: 16px; }
.table-title { margin: 18px 0 10px; color: #0f172a; font-size: 1rem; }
.table-topline .table-title { margin: 0; }
.form-row, .option-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; min-width: 0; }
.form-group { display: flex; flex-direction: column; gap: 8px; margin-bottom: 16px; min-width: 0; }
.glass-input, .glass-input-sm { width: 100%; max-width: 100%; box-sizing: border-box; background: #f8fafc; border: 1px solid #cbd5e1; border-radius: 8px; color: #1e293b; padding: 12px 16px; font-family: inherit; font-size: 0.95rem; }
.glass-input-sm { padding: 8px 12px; font-size: 0.85rem; width: 100%; }
.note-input { display: block; min-height: 44px; line-height: 1.45; resize: vertical; white-space: pre-wrap; overflow-wrap: anywhere; }
.thanh-phan-item { display: flex; gap: 12px; margin-bottom: 12px; align-items: center; padding: 12px; border-radius: 10px; background: #f8fafc; border: 1px solid #e2e8f0; }
.stt-badge { width: 36px; height: 36px; display: flex; align-items: center; justify-content: center; background: #fff7ed; color: #c2410c; border-radius: 50%; font-weight: 700; flex-shrink: 0; }
.flex-1 { flex: 1; }
.btn-icon { background: transparent; border: none; font-size: 1.5rem; cursor: pointer; display: inline-flex; align-items: center; justify-content: center; }
.remove-row-btn { width: 32px; height: 32px; margin: 0 auto; }
.text-red { color: #ef4444; }
.btn-outline { background: transparent; border: 1px solid #94a3b8; color: #475569; padding: 6px 12px; border-radius: 6px; cursor: pointer; font-size: 0.85rem; white-space: nowrap; }
.checkbox-row { display: flex; flex-wrap: wrap; gap: 14px; }
.checkbox-item { display: inline-flex; align-items: center; gap: 8px; font-weight: 500; color: #334155; }
.table-responsive { overflow-x: auto; margin-top: 16px; }
.requirement-table-wrap { margin-top: 10px; }
.glass-table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
.glass-table th, .glass-table td { padding: 12px; border: 1px solid #e2e8f0; vertical-align: middle; }
.glass-table th { background: #f8fafc; text-align: center; font-weight: 600; color: #475569; }
.meal-table th:nth-child(1), .meal-table td:nth-child(1) { width: 58px; }
.meal-table th:nth-child(2), .meal-table td:nth-child(2) { min-width: 240px; }
.meal-table th:nth-child(3), .meal-table th:nth-child(6) { min-width: 120px; }
.meal-table th:nth-child(4), .meal-table th:nth-child(5), .meal-table th:nth-child(7), .meal-table th:nth-child(8) { min-width: 86px; }
.meal-table th:nth-child(9), .meal-table td:nth-child(9) { min-width: 150px; }
.tube-table th:nth-child(9), .tube-table td:nth-child(9) { min-width: 52px; }
.requirement-table th:nth-child(1), .requirement-table td:nth-child(1) { width: 58px; }
.requirement-table th:nth-child(2), .requirement-table td:nth-child(2) { min-width: 360px; }
.requirement-table th:nth-child(3), .requirement-table th:nth-child(4) { width: 100px; }
.requirement-table th:nth-child(5), .requirement-table td:nth-child(5) { min-width: 160px; }
.text-center { text-align: center !important; }
.muted { margin-top: 6px; color: #64748b; font-size: 0.84rem; line-height: 1.45; }
.pre-line { white-space: pre-line; }
.form-actions { display: flex; justify-content: flex-end; gap: 15px; }
.btn-primary, .btn-secondary { padding: 12px 24px; border-radius: 8px; font-size: 1rem; font-weight: 600; cursor: pointer; display: flex; align-items: center; gap: 8px; border: none; }
.btn-primary { background: #0ea5e9; color: white; }
.btn-primary:disabled { opacity: 0.7; cursor: not-allowed; }
.btn-secondary { background: #f1f5f9; color: #475569; border: 1px solid #cbd5e1; }
.spinner { width: 20px; height: 20px; border: 3px solid rgba(255,255,255,0.3); border-top-color: #fff; border-radius: 50%; animation: spin 0.8s linear infinite; }

/* Floating Vertical Progress Bar on Desktop */
.sticky-progress-bar {
  position: fixed; right: 32px; top: 50%; transform: translateY(-50%); width: 72px;
  background: rgba(255,255,255,0.95); backdrop-filter: blur(12px); -webkit-backdrop-filter: blur(12px);
  border: 1px solid rgba(148,163,184,0.35); padding: 20px 10px;
  box-shadow: 0 12px 40px rgba(15,23,42,0.12); z-index: 999; border-radius: 40px;
  display: flex; flex-direction: column; align-items: center; gap: 16px;
  transition: all 0.3s cubic-bezier(0.4,0,0.2,1);
}
.sticky-progress-bar:hover { transform: translateY(-52%) scale(1.03); box-shadow: 0 16px 45px rgba(15,23,42,0.18); }
.progress-info { display: flex; flex-direction: column; align-items: center; gap: 14px; width: 100%; }
.progress-percent-circle {
  width: 52px; height: 52px; border-radius: 50%;
  background: linear-gradient(135deg,#f0fdf4,#e0f2fe); border: 1px solid rgba(56,189,248,0.2);
  display: flex; align-items: center; justify-content: center; box-shadow: 0 4px 10px rgba(15,23,42,0.04);
}
.progress-percent-circle span { font-size: 0.95rem; font-weight: 800; color: #0369a1; }
.progress-track {
  width: 8px; height: 120px; background: #f1f5f9; border-radius: 999px;
  overflow: hidden; position: relative; display: flex; align-items: flex-end;
}
.progress-fill {
  width: 100%; height: var(--progress-val);
  background: linear-gradient(180deg,#22c55e,#38bdf8); border-radius: 999px;
  transition: height 0.4s cubic-bezier(0.4,0,0.2,1);
}
.progress-text-fraction { display: flex; flex-direction: column; align-items: center; gap: 2px; text-align: center; }
.progress-text-fraction strong { font-size: 0.95rem; color: #0f172a; font-weight: 800; }
.progress-text-fraction span { font-size: 0.72rem; color: #64748b; font-weight: 500; text-transform: uppercase; letter-spacing: 0.5px; }
.btn-goto-missing {
  width: 44px; height: 44px; border-radius: 50%; background: #eff6ff; color: #0284c7;
  border: 1px solid #bae6fd; display: flex; align-items: center; justify-content: center;
  font-size: 1.25rem; cursor: pointer; position: relative; transition: all 0.2s ease;
  box-shadow: 0 4px 12px rgba(2,132,199,0.15);
}
.btn-goto-missing:hover { background: #0284c7; color: #fff; border-color: #0284c7; transform: scale(1.05); }
.btn-goto-missing .btn-tooltip {
  position: absolute; right: 60px; top: 50%; transform: translateY(-50%);
  background: #0f172a; color: #fff; padding: 6px 12px; border-radius: 8px;
  font-size: 0.8rem; font-weight: 600; white-space: nowrap; opacity: 0;
  pointer-events: none; transition: all 0.2s ease; box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}
.btn-goto-missing .btn-tooltip::after {
  content: ''; position: absolute; left: 100%; top: 50%; transform: translateY(-50%);
  border-width: 5px; border-style: solid; border-color: transparent transparent transparent #0f172a;
}
.btn-goto-missing:hover .btn-tooltip { opacity: 1; right: 54px; }
.progress-success-icon {
  width: 44px; height: 44px; border-radius: 50%; background: #f0fdf4; color: #16a34a;
  border: 1px solid #bbf7d0; display: flex; align-items: center; justify-content: center;
  font-size: 1.5rem; box-shadow: 0 4px 12px rgba(22,163,74,0.15);
}
.btn-goto-missing .btn-text, .progress-success-icon .btn-text { display: none; }

@media (max-width: 960px) {
  .sticky-progress-bar {
    position: fixed; bottom: 0; left: 0; right: 0; top: auto; width: 100%; transform: none;
    border-radius: 16px 16px 0 0; border: none; border-top: 1px solid #e2e8f0;
    padding: 12px 20px; box-shadow: 0 -5px 25px rgba(0,0,0,0.08);
    flex-direction: row; justify-content: space-between; height: auto; gap: 0;
  }
  .sticky-progress-bar:hover { transform: none; }
  .progress-info { flex-direction: row; align-items: center; justify-content: space-between; gap: 12px; width: auto; }
  .progress-percent-circle { width: 38px; height: 38px; flex-shrink: 0; }
  .progress-percent-circle span { font-size: 0.8rem; }
  .progress-track { display: none; }
  .progress-text-fraction { flex-direction: row; align-items: center; gap: 4px; }
  .progress-text-fraction strong { font-size: 0.9rem; }
  .progress-text-fraction span { font-size: 0.8rem; }
  .btn-goto-missing, .progress-success-icon { width: auto; height: auto; border-radius: 10px; padding: 8px 16px; font-size: 0.85rem; font-weight: 700; box-shadow: none; }
  .btn-goto-missing ion-icon, .progress-success-icon ion-icon { font-size: 1.1rem; }
  .btn-goto-missing .btn-tooltip { display: none; }
  .btn-goto-missing .btn-text, .progress-success-icon .btn-text { display: inline; font-size: 0.85rem; margin-left: 6px; }
}

/* Flash Highlight Animation for incomplete item row */
:global(tr.flash-highlight) {
  animation: rowFlash 1.6s ease-in-out infinite;
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

@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 768px) {
  .form-container { padding-bottom: max(320px, env(safe-area-inset-bottom)); }
  .form-row, .option-grid { grid-template-columns: minmax(0, 1fr); width: 100%; max-width: 100%; }
  .glass-card { overflow: hidden; padding: 18px; }
  input[type='date'].glass-input { width: 100%; max-width: 100%; min-width: 0; -webkit-appearance: none; appearance: none; }
  .thanh-phan-item { flex-direction: column; align-items: stretch; gap: 8px; }
  .form-actions { flex-direction: column; align-items: stretch; }
  .section-topline { align-items: flex-start; flex-direction: column; }
  .table-responsive { overflow-x: auto; -webkit-overflow-scrolling: touch; padding-bottom: 8px; }
  .glass-table { min-width: 880px; }
  .requirement-table { min-width: 760px; }
  .note-input { min-width: 180px; min-height: 72px; font-size: 0.95rem; }
  .glass-input:focus, .glass-input-sm:focus { border-color: #38bdf8; box-shadow: 0 0 0 3px rgba(14, 165, 233, 0.18); outline: none; }
}
/* Nội dung cell with dotted lines (matching paper form) */
.noi-dung-cell { vertical-align: top !important; }
.noi-dung-cell strong { color: #0f172a; font-size: 0.9rem; display: block; margin-bottom: 2px; }
.dotted-lines { display: flex; flex-direction: column; gap: 0; margin-top: 4px; }
.dotted-line-item { display: flex; align-items: center; }
.dotted-line-input {
  width: 100%;
  border: none;
  border-bottom: 1px dashed #94a3b8;
  background: transparent;
  padding: 4px 2px;
  font-family: inherit;
  font-size: 0.85rem;
  color: #1e293b;
  outline: none;
  line-height: 1.5;
}
.dotted-line-input::placeholder { color: #cbd5e1; letter-spacing: 1px; font-size: 0.8rem; }
.dotted-line-input:focus { border-bottom-color: #0ea5e9; background: rgba(14, 165, 233, 0.04); }
</style>
