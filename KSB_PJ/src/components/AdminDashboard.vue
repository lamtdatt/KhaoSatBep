<script setup>
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import AppToast from '@/components/AppToast.vue'
import SignaturePadPage from '@/components/SignaturePadPage.vue'
import logoUrl from '@/assets/logo.png'
import { clearAuthSession } from '@/utils/authStore'
import { getReportById, getReportStats, getReports, markReportRead, refreshReports, updateReport } from '@/utils/reportStore'
import { getSignatureProfile, saveSignatureProfile } from '@/utils/signatureStore'
import { getDefaultReportTemplate, getReportTemplate, saveReportTemplate } from '@/utils/templateStore'

const router = useRouter()
const reports = ref([])
const selectedId = ref('')
const selectedReport = ref(null)
const activeSection = ref('dashboard')
const isEditingReport = ref(false)
const selectedTemplateType = ref('')
const templateEditorDetails = ref([])
const reportSearch = ref('')
const reportDateFrom = ref('')
const reportDateTo = ref('')
const adminMainRef = ref(null)
const selectedReportSectionRef = ref(null)
const adminName = ref('Admin duyệt biên bản')
const adminRole = ref('Quản lý Khoa Dinh Dưỡng')
const adminSignature = ref(null)
const employeeSignature = ref(null)
const canvasRef = ref(null)
const isAdminIntroLoading = ref(true)
const adminIntroProgress = ref(8)
const showMobileBackTop = ref(false)
const toast = ref({
  visible: false,
  message: ''
})

let drawing = false
let ctx = null
let toastTimer = null
let introProgressTimer = null
let introFinishTimer = null

const showToast = message => {
  toast.value = {
    visible: true,
    message
  }

  window.clearTimeout(toastTimer)
  toastTimer = window.setTimeout(() => {
    toast.value.visible = false
  }, 2800)
}

const logout = () => {
  clearAuthSession()
  router.push('/login')
}

const startAdminIntro = () => {
  isAdminIntroLoading.value = true
  adminIntroProgress.value = 8

  window.clearInterval(introProgressTimer)
  window.clearTimeout(introFinishTimer)

  introProgressTimer = window.setInterval(() => {
    adminIntroProgress.value = Math.min(adminIntroProgress.value + 7, 92)
  }, 90)

  introFinishTimer = window.setTimeout(() => {
    window.clearInterval(introProgressTimer)
    adminIntroProgress.value = 100

    window.setTimeout(() => {
      isAdminIntroLoading.value = false
    }, 260)
  }, 1100)
}

const updateMobileBackTop = () => {
  showMobileBackTop.value = window.innerWidth <= 720 && window.scrollY > 260
}

const reportTypeLabels = {
  CoSoHaTang: 'Cơ sở hạ tầng',
  HoSo: 'Hồ sơ sổ sách',
  VeSinh: 'Vệ sinh ATTP',
  SuatAnNguoiBenh: 'Suất ăn người bệnh'
}

const loadReports = async () => {
  await refreshReports()
  const nextReports = getReports()
  reports.value = nextReports

  if (selectedId.value) {
    const fresh = nextReports.find(report => report.id === selectedId.value)
    if (fresh && !selectedReport.value) {
      selectedReport.value = structuredClone(fresh)
    }
    if (!fresh) {
      selectedId.value = ''
      selectedReport.value = null
    }
    return
  }
}

const activeReport = computed(() => {
  return selectedReport.value || reports.value.find(report => report.id === selectedId.value) || null
})

const selectedStats = computed(() => getReportStats(activeReport.value))
const unreadCount = computed(() => reports.value.filter(report => !report.readByAdmin).length)
const totalSubmitted = computed(() => reports.value.length)
const approvedCount = computed(() => reports.value.filter(report => report.status === 'approved').length)
const pendingCount = computed(() => reports.value.filter(report => report.status !== 'approved').length)
const submittedCount = computed(() => reports.value.filter(report => report.status === 'submitted').length)
const reviewedCount = computed(() => reports.value.filter(report => report.status === 'reviewed').length)

const totalCheckedItems = computed(() => {
  return reports.value.reduce((sum, report) => sum + getReportStats(report).total, 0)
})

const totalPassedItems = computed(() => {
  return reports.value.reduce((sum, report) => sum + getReportStats(report).dat, 0)
})

const compliancePercent = computed(() => {
  return totalCheckedItems.value ? Math.round((totalPassedItems.value / totalCheckedItems.value) * 100) : 0
})

const maxModuleCount = computed(() => Math.max(...typeStats.value.map(stat => stat.count), 1))

const recentReports = computed(() => {
  return reports.value.slice(0, 6).map(report => ({
    ...report,
    label: reportTypeLabels[report.loaiBienBan] || report.loaiBienBan,
    time: new Date(report.submittedAt || report.updatedAt).toLocaleString('vi-VN')
  }))
})

const statusStats = computed(() => [
  {
    label: 'Chờ duyệt',
    value: submittedCount.value,
    color: '#f59e0b'
  },
  {
    label: 'Đã duyệt',
    value: approvedCount.value,
    color: '#16a34a'
  },
  {
    label: 'Đã chỉnh sửa',
    value: reviewedCount.value,
    color: '#0ea5e9'
  }
])

const maxStatusCount = computed(() => Math.max(...statusStats.value.map(stat => stat.value), 1))

const dashboardCards = computed(() => [
  {
    label: 'Tổng biên bản',
    value: totalSubmitted.value,
    icon: 'folder-open-outline',
    note: 'Tất cả biên bản đã nhận'
  },
  {
    label: 'Chờ duyệt',
    value: submittedCount.value,
    icon: 'time-outline',
    note: 'Cần admin xử lý'
  },
  {
    label: 'Tỉ lệ đạt',
    value: `${compliancePercent.value}%`,
    icon: 'analytics-outline',
    note: `${totalPassedItems.value}/${totalCheckedItems.value} tiêu chí đạt`
  },
  {
    label: 'Thông báo mới',
    value: unreadCount.value,
    icon: 'notifications-outline',
    note: 'Biên bản chưa mở'
  }
])

const getReportDate = report => {
  return report.submittedAt || report.updatedAt || report.ngayKiemTra || ''
}

const isReportInDateRange = report => {
  const rawDate = getReportDate(report)
  if (!rawDate) {
    return true
  }

  const reportDate = new Date(rawDate)
  const fromDate = reportDateFrom.value ? new Date(`${reportDateFrom.value}T00:00:00`) : null
  const toDate = reportDateTo.value ? new Date(`${reportDateTo.value}T23:59:59`) : null

  if (fromDate && reportDate < fromDate) {
    return false
  }

  if (toDate && reportDate > toDate) {
    return false
  }

  return true
}

const filteredReports = computed(() => {
  const keyword = reportSearch.value.trim().toLowerCase()

  return reports.value.filter(report => {
    const text = [
      report.soBienBan,
      reportTypeLabels[report.loaiBienBan],
      report.status,
      report.ngayKiemTra
    ].join(' ').toLowerCase()

    return (!keyword || text.includes(keyword)) && isReportInDateRange(report)
  })
})

const reportListGroups = computed(() => {
  return Object.entries(reportTypeLabels).map(([type, label]) => ({
    type,
    label,
    reports: filteredReports.value.filter(report => report.loaiBienBan === type)
  }))
})

const resetReportFilters = () => {
  reportSearch.value = ''
  reportDateFrom.value = ''
  reportDateTo.value = ''
}

const loadTemplateEditor = type => {
  selectedTemplateType.value = type

  const savedTemplate = getReportTemplate(type)
  if (savedTemplate?.items?.length) {
    templateEditorDetails.value = savedTemplate.items.map(item => ({
      mucSo: item.mucSo,
      phanNhom: item.phanNhom || '',
      noiDung: item.noiDung || ''
    }))
    return
  }

  const sourceReport = reports.value.find(report => report.loaiBienBan === type)
  const sourceDetails = sourceReport?.chiTiets?.length
    ? sourceReport.chiTiets
    : getDefaultReportTemplate(type)?.items || []

  templateEditorDetails.value = sourceDetails.map(detail => ({
    mucSo: detail.mucSo,
    phanNhom: detail.phanNhom || '',
    noiDung: detail.noiDung || ''
  }))
}

const openTemplateManager = type => {
  if (!type) {
    return
  }

  loadTemplateEditor(type)
  openSection('templateManager')
}

const updateTemplate = () => {
  if (!selectedTemplateType.value) {
    return
  }

  saveReportTemplate(selectedTemplateType.value, templateEditorDetails.value)
  showToast('Đã cập nhật mẫu đơn. Nhân viên mở form sẽ thấy nội dung mới.')
}

const templateMeta = computed(() => {
  const meta = {
    CoSoHaTang: {
      title: 'Biên bản kiểm tra Cơ sở hạ tầng, trang thiết bị',
      subtitle: 'Tại bộ phận chế biến và cung cấp suất ăn'
    },
    HoSo: {
      title: 'Biên bản kiểm tra Hồ sơ, sổ sách, các chứng từ',
      subtitle: 'Tại bộ phận chế biến và cung cấp suất ăn'
    },
    VeSinh: {
      title: 'Biên bản kiểm tra Vệ sinh An toàn Thực phẩm',
      subtitle: 'Khoa Dinh Dưỡng - BV Hoàn Mỹ Đồng Nai'
    },
    SuatAnNguoiBenh: {
      title: 'Biên bản kiểm tra Suất ăn người bệnh',
      subtitle: 'Tại bộ phận chế biến và cung cấp suất ăn'
    }
  }

  return meta[selectedTemplateType.value] || {
    title: 'Mẫu biên bản',
    subtitle: 'Chọn một loại biên bản để chỉnh sửa mẫu'
  }
})

const templateSections = computed(() => {
  const groups = new Map()

  templateEditorDetails.value.forEach((detail, index) => {
    const key = detail.phanNhom || 'Nội dung kiểm tra'
    if (!groups.has(key)) {
      groups.set(key, [])
    }

    groups.get(key).push({
      ...detail,
      index
    })
  })

  return Array.from(groups.entries()).map(([title, items]) => ({
    title,
    items
  }))
})

const addTemplateDetail = phanNhom => {
  const maxIndex = templateEditorDetails.value.reduce((max, detail) => {
    return Math.max(max, Number(detail.mucSo) || 0)
  }, 0)

  templateEditorDetails.value.push({
    mucSo: maxIndex + 1,
    phanNhom: phanNhom === 'Nội dung kiểm tra' ? '' : phanNhom,
    noiDung: 'Tiêu chí mới'
  })
}

const removeTemplateDetail = index => {
  templateEditorDetails.value.splice(index, 1)
}

const scrollMainToTop = () => {
  if (window.innerWidth <= 720) {
    window.scrollTo({
      top: adminMainRef.value?.offsetTop || 0,
      behavior: 'smooth'
    })
    return
  }

  adminMainRef.value?.scrollTo({
    top: 0,
    behavior: 'smooth'
  })
}

const openSection = async section => {
  activeSection.value = section
  await nextTick()
  scrollMainToTop()
}

const scrollToAdminTop = () => {
  window.scrollTo({
    top: 0,
    behavior: 'smooth'
  })
}

const typeStats = computed(() => {
  return Object.entries(reportTypeLabels).map(([type, label]) => {
    const sameType = reports.value.filter(report => report.loaiBienBan === type)
    const totals = sameType.reduce(
      (sum, report) => {
        const stats = getReportStats(report)
        return {
          dat: sum.dat + stats.dat,
          khongDat: sum.khongDat + stats.khongDat,
          total: sum.total + stats.total
        }
      },
      { dat: 0, khongDat: 0, total: 0 }
    )

    return {
      type,
      label,
      count: sameType.length,
      datPercent: totals.total ? Math.round((totals.dat / totals.total) * 100) : 0,
      khongDatPercent: totals.total ? Math.round((totals.khongDat / totals.total) * 100) : 0
    }
  })
})

const selectReport = async report => {
  activeSection.value = 'reports'
  selectedId.value = report.id
  const currentReport = await getReportById(report.id)
  selectedReport.value = structuredClone(currentReport)
  isEditingReport.value = false

  if (!report.readByAdmin) {
    await markReportRead(report.id)
    reports.value = getReports()
  }

  await nextTick()
  scrollMainToTop()
}

const startEditReport = () => {
  if (!activeReport.value) {
    return
  }

  selectedReport.value = structuredClone(activeReport.value)
  isEditingReport.value = true
}

const cancelEditReport = () => {
  const fresh = getReports().find(report => report.id === selectedId.value)
  selectedReport.value = fresh ? structuredClone(fresh) : null
  isEditingReport.value = false
}

const saveEdits = () => {
  if (!activeReport.value) {
    return
  }

  saveReportTemplate(activeReport.value.loaiBienBan, activeReport.value.chiTiets)
  const fresh = getReports().find(report => report.id === activeReport.value.id)
  selectedReport.value = fresh ? structuredClone(fresh) : null
  isEditingReport.value = false
  window.dispatchEvent(new CustomEvent('ksb-templates-updated'))
  showToast('Đã lưu mẫu biên bản. Nhân viên mở form sẽ thấy mẫu mới.')
}

const approveReport = async () => {
  if (!activeReport.value) {
    return
  }

  const updated = await updateReport(activeReport.value.id, {
    status: 'approved',
    approvedAt: new Date().toISOString()
  })

  if (updated) {
    selectedReport.value = structuredClone(updated)
    await loadReports()
    showToast('Đã duyệt biên bản thành công.')
  }
}

const setupCanvas = () => {
  const canvas = canvasRef.value
  if (!canvas) {
    return
  }

  const ratio = window.devicePixelRatio || 1
  const width = canvas.offsetWidth || 420
  const height = canvas.offsetHeight || 180
  canvas.width = width * ratio
  canvas.height = height * ratio

  ctx = canvas.getContext('2d')
  ctx.setTransform(1, 0, 0, 1, 0, 0)
  ctx.scale(ratio, ratio)
  ctx.lineCap = 'round'
  ctx.lineJoin = 'round'
  ctx.lineWidth = 2.4
  ctx.strokeStyle = '#0f172a'
  ctx.fillStyle = '#ffffff'
  ctx.fillRect(0, 0, width, height)

  if (adminSignature.value?.imageData) {
    const image = new Image()
    image.onload = () => {
      ctx.fillStyle = '#ffffff'
      ctx.fillRect(0, 0, width, height)
      ctx.drawImage(image, 0, 0, width, height)
    }
    image.src = adminSignature.value.imageData
  }
}

const getCanvasPoint = event => {
  const rect = canvasRef.value.getBoundingClientRect()
  const source = event.touches?.[0] ?? event
  return {
    x: source.clientX - rect.left,
    y: source.clientY - rect.top
  }
}

const startDrawing = event => {
  if (!ctx) {
    return
  }
  drawing = true
  const point = getCanvasPoint(event)
  ctx.beginPath()
  ctx.moveTo(point.x, point.y)
  event.preventDefault()
}

const draw = event => {
  if (!drawing || !ctx) {
    return
  }
  const point = getCanvasPoint(event)
  ctx.lineTo(point.x, point.y)
  ctx.stroke()
  event.preventDefault()
}

const stopDrawing = () => {
  drawing = false
}

const clearCanvas = () => {
  if (!canvasRef.value || !ctx) {
    return
  }
  const width = canvasRef.value.offsetWidth || 420
  const height = canvasRef.value.offsetHeight || 180
  ctx.clearRect(0, 0, width, height)
  ctx.fillStyle = '#ffffff'
  ctx.fillRect(0, 0, width, height)
}

const saveAdminSignature = () => {
  if (!canvasRef.value) {
    return
  }

  const profile = {
    name: adminName.value.trim() || 'Admin duyệt biên bản',
    role: adminRole.value.trim() || 'Quản lý Khoa Dinh Dưỡng',
    imageData: canvasRef.value.toDataURL('image/png'),
    updatedAt: new Date().toISOString()
  }

  saveSignatureProfile(profile, 'admin')
  adminSignature.value = profile
  showToast('Đã lưu chữ ký admin.')
}

const escapeHtml = value => String(value ?? '')
  .replace(/&/g, '&amp;')
  .replace(/</g, '&lt;')
  .replace(/>/g, '&gt;')
  .replace(/"/g, '&quot;')
  .replace(/'/g, '&#039;')

const formatDate = value => {
  if (!value) {
    return 'Chưa cập nhật'
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleDateString('vi-VN')
}

const formatDateTime = value => {
  if (!value) {
    return 'Chưa cập nhật'
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleString('vi-VN')
}

const getStatusLabel = status => ({
  submitted: 'Chờ duyệt',
  reviewed: 'Đã chỉnh sửa',
  approved: 'Đã duyệt'
})[status] || status || 'Chưa cập nhật'

const getResultText = item => {
  const values = [item.dat, item.cheDo1Dat, item.cheDo2Dat].filter(value => value === true || value === false)
  if (!values.length) {
    return ''
  }

  const passed = values.filter(Boolean).length
  const failed = values.length - passed

  if (item.dat === true) {
    return 'Đạt'
  }

  if (item.dat === false) {
    return 'Không đạt'
  }

  return `Đạt ${passed}/${values.length}${failed ? `, Không đạt ${failed}/${values.length}` : ''}`
}

const getResultClass = item => {
  const values = [item.dat, item.cheDo1Dat, item.cheDo2Dat].filter(value => value === true || value === false)
  if (!values.length) {
    return 'result-empty'
  }

  if (values.every(Boolean)) {
    return 'result-pass'
  }

  if (values.every(value => value === false)) {
    return 'result-fail'
  }

  return 'result-mixed'
}

const getReportNote = item => {
  const parts = [
    item.ghiChuNoiDung ? `Mô tả: ${item.ghiChuNoiDung}` : '',
    item.cheDo1KhoiLuong ? `CĐ1: ${item.cheDo1KhoiLuong}` : '',
    item.cheDo2KhoiLuong ? `CĐ2: ${item.cheDo2KhoiLuong}` : '',
    item.ghiChu || ''
  ].filter(Boolean)

  return parts.join(' | ')
}

const renderSignature = (profile, title, signedAt = '') => {
  const image = profile?.imageData
    ? `<img src="${profile.imageData}" alt="${escapeHtml(title)}" />`
    : '<div class="missing-signature">Chưa có chữ ký</div>'

  return `
    <div class="signature-box">
      <strong>${escapeHtml(title)}</strong>
      <span class="signature-date">${escapeHtml(signedAt)}</span>
      ${image}
      <b>${escapeHtml(profile?.name || '')}</b>
      <span>${escapeHtml(profile?.role || '')}</span>
    </div>
  `
}

const exportPdf = () => {
  if (!activeReport.value) {
    return
  }

  const employeeSignature = getSignatureProfile('employee')
  const currentAdminSignature = getSignatureProfile('admin')
  const report = activeReport.value
  const details = report.chiTiets || []
  const approvedAt = report.approvedAt || (report.status === 'approved' ? report.updatedAt : '')
  const exportedAt = new Date().toISOString()
  const rows = details.map(item => `
    <tr>
      <td class="text-center">${escapeHtml(item.mucSo)}</td>
      <td>${escapeHtml(item.phanNhom || '-')}</td>
      <td>${escapeHtml(item.noiDung)}</td>
      <td class="text-center result-cell">${escapeHtml(getResultText(item))}</td>
      <td>${escapeHtml(getReportNote(item) || '-')}</td>
    </tr>
  `).join('')
  const participantRows = (report.thanhPhans || []).filter(item => item.hoTen || item.chucVu).map(item => `
    <tr>
      <td class="text-center">${escapeHtml(item.stt)}</td>
      <td>${escapeHtml(item.hoTen)}</td>
      <td>${escapeHtml(item.chucVu)}</td>
    </tr>
  `).join('')

  const stats = getReportStats(report)
  const printWindow = window.open('', '_blank')
  if (!printWindow) {
    showToast('Trình duyệt đang chặn cửa sổ xuất PDF. Vui lòng cho phép pop-up rồi thử lại.')
    return
  }
  printWindow.document.write(`
    <!doctype html>
    <html>
      <head>
        <title>${escapeHtml(report.soBienBan || 'Bien-ban')}</title>
        <style>
          @page { size: A4; margin: 14mm 12mm; }
          * { box-sizing: border-box; }
          body { margin: 0; font-family: "Times New Roman", Times, serif; color: #111827; background: #ffffff; font-size: 12px; }
          .document { max-width: 980px; margin: 0 auto; }
          .letterhead { display: grid; grid-template-columns: 92px 1fr 220px; gap: 16px; align-items: center; padding-bottom: 12px; border-bottom: 2px solid #0f766e; }
          .logo { width: 78px; height: 78px; object-fit: contain; }
          .brand strong { display: block; color: #0f766e; font-size: 16px; text-transform: uppercase; }
          .brand span { display: block; margin-top: 4px; color: #475569; }
          .doc-code { text-align: right; color: #334155; line-height: 1.6; }
          .doc-code b { color: #111827; }
          .national { margin: 14px 0 8px; text-align: center; line-height: 1.45; }
          .national strong { display: block; text-transform: uppercase; }
          h1 { margin: 18px 0 6px; text-align: center; font-size: 22px; font-weight: 800; letter-spacing: 0.01em; text-transform: uppercase; }
          .subtitle { margin: 0 0 18px; text-align: center; color: #475569; font-size: 12px; font-weight: 600; }
          .meta-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 8px; margin: 14px 0; }
          .meta-card { min-height: 58px; padding: 9px 10px; border: 1px solid #cbd5e1; border-radius: 6px; }
          .meta-card span { display: block; color: #64748b; font-size: 11px; }
          .meta-card b { display: block; margin-top: 5px; font-size: 13px; }
          .section-title { margin: 18px 0 8px; font-size: 14px; font-weight: 800; text-transform: uppercase; }
          table { width: 100%; border-collapse: collapse; table-layout: fixed; page-break-inside: auto; }
          thead { display: table-header-group; }
          tr { page-break-inside: avoid; page-break-after: auto; }
          th, td { border: 1px solid #9aaec9; padding: 8px 9px; vertical-align: top; word-break: break-word; }
          th { background: #f3f8fb; color: #111827; text-align: center; font-size: 11px; font-weight: 650; letter-spacing: 0; }
          td { color: #1f2937; line-height: 1.5; font-size: 11.25px; font-weight: 400; }
          td:nth-child(2) { font-weight: 400; }
          td:nth-child(4) { color: #334155; }
          .text-center { text-align: center; }
          .result-cell { font-size: 11px; font-weight: 650; white-space: nowrap; }
          .result-pass { color: #036f8f; }
          .result-fail { color: #b91c1c; }
          .result-mixed { color: #92400e; }
          .result-empty { color: #64748b; }
          .summary { display: grid; grid-template-columns: repeat(4, 1fr); gap: 8px; margin: 14px 0; }
          .summary div { padding: 10px; border: 1px solid #cbd5e1; border-radius: 6px; background: #f8fafc; }
          .summary span { display: block; color: #64748b; font-size: 11px; }
          .summary b { display: block; margin-top: 5px; font-size: 18px; }
          .opinion-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }
          .opinion-box { min-height: 76px; padding: 10px; border: 1px solid #cbd5e1; border-radius: 6px; }
          .opinion-box strong { display: block; margin-bottom: 6px; }
          .signatures { display: grid; grid-template-columns: 1fr 1fr; gap: 42px; margin-top: 34px; text-align: center; page-break-inside: avoid; }
          .signature-box strong, .signature-box b, .signature-box span { display: block; margin-top: 4px; }
          .signature-date { min-height: 16px; color: #64748b; font-size: 11px; }
          .signature-box img { display: block; width: 230px; height: 96px; object-fit: contain; margin: 12px auto 8px; border-bottom: 1px solid #cbd5e1; }
          .missing-signature { height: 96px; margin: 12px 0 8px; color: #64748b; display: grid; place-items: center; border-bottom: 1px solid #cbd5e1; }
          .footer { margin-top: 24px; padding-top: 8px; border-top: 1px solid #cbd5e1; color: #64748b; font-size: 10px; display: flex; justify-content: space-between; gap: 12px; }
          @media print { .document { max-width: none; } }
        </style>
      </head>
      <body>
        <h1>${escapeHtml(reportTypeLabels[report.loaiBienBan] || report.loaiBienBan)}</h1>
        <div class="meta">
          <div><strong>Số biên bản:</strong> ${escapeHtml(report.soBienBan)}</div>
          <div><strong>Ngày kiểm tra:</strong> ${escapeHtml(report.ngayKiemTra)}</div>
          <div><strong>Trạng thái:</strong> ${escapeHtml(report.status)}</div>
        </div>
        <div class="stats">Đạt: ${stats.datPercent}% | Không đạt: ${stats.khongDatPercent}% | Đã chấm: ${stats.total}</div>
        <table>
          <thead>
            <tr>
              <th>TT</th>
              <th>Phần</th>
              <th>Nội dung</th>
              <th>Đạt</th>
              <th>K.Đạt</th>
              <th>Ghi chú</th>
            </tr>
          </thead>
          <tbody>${rows}</tbody>
        </table>
        <div class="signatures">
          ${renderSignature(employeeSignature, 'Nhân viên khảo sát')}
          ${renderSignature(currentAdminSignature, 'Admin duyệt')}
        </div>
        <script>window.onload = () => { window.print() }<\/script>
      </body>
    </html>
  `)
  printWindow.document.close()
}

const exportPdfAdvanced = async () => {
  if (!activeReport.value) {
    return
  }

  const employeeProfile = getSignatureProfile('employee')
  const adminProfile = getSignatureProfile('admin')
  const report = activeReport.value
  const stats = getReportStats(report)
  const details = report.chiTiets || []
  const approvedAt = report.approvedAt || (report.status === 'approved' ? report.updatedAt : '')
  const exportedAt = new Date().toISOString()
  const rows = details.map(item => `
    <tr>
      <td class="text-center">${escapeHtml(item.mucSo)}</td>
      <td>${escapeHtml(item.noiDung)}</td>
      <td class="text-center result-cell ${getResultClass(item)}">${escapeHtml(getResultText(item) || '-')}</td>
      <td>${escapeHtml(getReportNote(item) || '-')}</td>
    </tr>
  `).join('')
  const participantRows = (report.thanhPhans || [])
    .filter(item => item.hoTen || item.chucVu)
    .map(item => `
      <tr>
        <td class="text-center">${escapeHtml(item.stt)}</td>
        <td>${escapeHtml(item.hoTen)}</td>
        <td>${escapeHtml(item.chucVu)}</td>
      </tr>
    `).join('')

  const printWindow = window.open('', '_blank')
  if (!printWindow) {
    showToast('Trình duyệt đang chặn cửa sổ xuất PDF. Vui lòng cho phép pop-up rồi thử lại.')
    return
  }

  printWindow.document.write(`
    <!doctype html>
    <html>
      <head>
        <title>${escapeHtml(report.soBienBan || 'Bien-ban')}</title>
        <style>
          @page { size: A4; margin: 14mm 12mm; }
          * { box-sizing: border-box; }
          body { margin: 0; font-family: "Times New Roman", Times, serif; color: #111827; background: #ffffff; font-size: 12px; }
          .document { max-width: 980px; margin: 0 auto; }
          .letterhead { display: grid; grid-template-columns: 92px 1fr 220px; gap: 16px; align-items: center; padding-bottom: 12px; border-bottom: 2px solid #0f766e; }
          .logo { width: 78px; height: 78px; object-fit: contain; }
          .brand strong { display: block; color: #0f766e; font-size: 16px; text-transform: uppercase; }
          .brand span { display: block; margin-top: 4px; color: #475569; }
          .doc-code { text-align: right; color: #334155; line-height: 1.6; }
          .doc-code b { color: #111827; }
          .national { margin: 14px 0 8px; text-align: center; line-height: 1.45; }
          .national strong { display: block; text-transform: uppercase; }
          h1 { margin: 18px 0 6px; text-align: center; font-size: 22px; font-weight: 700; letter-spacing: 0; text-transform: uppercase; }
          .subtitle { margin: 0 0 18px; text-align: center; color: #475569; font-size: 12px; font-weight: 400; }
          .meta-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 8px; margin: 14px 0; }
          .meta-card { min-height: 58px; padding: 9px 10px; border: 1px solid #cbd5e1; border-radius: 6px; }
          .meta-card span { display: block; color: #64748b; font-size: 11px; }
          .meta-card b { display: block; margin-top: 5px; font-size: 13px; }
          .section-title { margin: 18px 0 8px; font-size: 14px; font-weight: 700; text-transform: uppercase; }
          table { width: 100%; border-collapse: collapse; table-layout: fixed; page-break-inside: auto; }
          thead { display: table-header-group; }
          tr { page-break-inside: avoid; page-break-after: auto; }
          th, td { border: 1px solid #9aaec9; padding: 8px 9px; vertical-align: top; word-break: break-word; }
          th { background: #f3f8fb; color: #111827; text-align: center; font-size: 11px; font-weight: 600; letter-spacing: 0; }
          td { color: #1f2937; line-height: 1.5; font-size: 11.25px; font-weight: 400; }
          td:nth-child(2) { font-weight: 400; }
          td:nth-child(4) { color: #334155; }
          .text-center { text-align: center; }
          .result-cell { font-size: 11px; font-weight: 600; white-space: nowrap; }
          .result-pass { color: #036f8f; }
          .result-fail { color: #b91c1c; }
          .result-mixed { color: #92400e; }
          .result-empty { color: #64748b; }
          .summary { display: grid; grid-template-columns: repeat(4, 1fr); gap: 8px; margin: 14px 0; }
          .summary div { padding: 10px; border: 1px solid #cbd5e1; border-radius: 6px; background: #f8fafc; }
          .summary span { display: block; color: #64748b; font-size: 11px; }
          .summary b { display: block; margin-top: 5px; font-size: 18px; }
          .opinion-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }
          .opinion-box { min-height: 76px; padding: 10px; border: 1px solid #cbd5e1; border-radius: 6px; }
          .opinion-box strong { display: block; margin-bottom: 6px; }
          .signatures { display: grid; grid-template-columns: 1fr 1fr; gap: 42px; margin-top: 34px; text-align: center; page-break-inside: avoid; }
          .signature-box strong, .signature-box b, .signature-box span { display: block; margin-top: 4px; }
          .signature-date { min-height: 16px; color: #64748b; font-size: 11px; }
          .signature-box img { display: block; width: 230px; height: 96px; object-fit: contain; margin: 12px auto 8px; border-bottom: 1px solid #cbd5e1; }
          .missing-signature { height: 96px; margin: 12px 0 8px; color: #64748b; display: grid; place-items: center; border-bottom: 1px solid #cbd5e1; }
          .footer { margin-top: 24px; padding-top: 8px; border-top: 1px solid #cbd5e1; color: #64748b; font-size: 10px; display: flex; justify-content: space-between; gap: 12px; }
          @media print { .document { max-width: none; } }
        </style>
      </head>
      <body>
        <main class="document">
          <header class="letterhead">
            <img class="logo" src="${logoUrl}" alt="Logo" />
            <div class="brand">
              <strong>Khảo Sát Bếp</strong>
              <span>Hệ thống biểu mẫu nội bộ</span>
              <span>Khoa Dinh Dưỡng - Bộ phận chế biến và cung cấp suất ăn</span>
            </div>
            <div class="doc-code">
              <div>Mã biên bản: <b>${escapeHtml(report.soBienBan || report.id)}</b></div>
              <div>Ngày xuất: <b>${escapeHtml(formatDateTime(exportedAt))}</b></div>
              <div>Phiên bản: <b>PDF-01</b></div>
            </div>
          </header>

          <div class="national">
            <strong>Cộng hòa xã hội chủ nghĩa Việt Nam</strong>
            <span>Độc lập - Tự do - Hạnh phúc</span>
          </div>

          <h1>${escapeHtml(reportTypeLabels[report.loaiBienBan] || report.loaiBienBan)}</h1>
          <p class="subtitle">Biên bản kiểm tra nội bộ dùng để lưu hồ sơ và theo dõi chất lượng bếp</p>

          <section class="meta-grid">
            <div class="meta-card"><span>Số biên bản</span><b>${escapeHtml(report.soBienBan || report.id)}</b></div>
            <div class="meta-card"><span>Ngày kiểm tra</span><b>${escapeHtml(formatDate(report.ngayKiemTra))}</b></div>
            <div class="meta-card"><span>Ngày gửi</span><b>${escapeHtml(formatDateTime(report.submittedAt))}</b></div>
            <div class="meta-card"><span>Ngày duyệt</span><b>${escapeHtml(formatDateTime(approvedAt))}</b></div>
            <div class="meta-card"><span>Trạng thái</span><b>${escapeHtml(getStatusLabel(report.status))}</b></div>
            <div class="meta-card"><span>Loại biên bản</span><b>${escapeHtml(reportTypeLabels[report.loaiBienBan] || report.loaiBienBan)}</b></div>
            <div class="meta-card"><span>Người khảo sát</span><b>${escapeHtml(employeeProfile?.name || 'Nhân viên khảo sát')}</b></div>
            <div class="meta-card"><span>Người duyệt</span><b>${escapeHtml(adminProfile?.name || adminName.value)}</b></div>
          </section>

          ${participantRows ? `
            <h2 class="section-title">I. Thành phần tham gia</h2>
            <table>
              <thead><tr><th style="width: 64px;">STT</th><th>Họ và tên</th><th>Chức vụ</th></tr></thead>
              <tbody>${participantRows}</tbody>
            </table>
          ` : ''}

          <h2 class="section-title">II. Tổng hợp kết quả</h2>
          <section class="summary">
            <div><span>Tổng mục đã chấm</span><b>${stats.total}</b></div>
            <div><span>Đạt</span><b>${stats.dat}</b></div>
            <div><span>Không đạt</span><b>${stats.khongDat}</b></div>
            <div><span>Tỷ lệ đạt</span><b>${stats.datPercent}%</b></div>
          </section>

          <h2 class="section-title">III. Nội dung kiểm tra</h2>
          <table>
            <thead>
              <tr>
                <th style="width: 52px;">TT</th>
                <th>Nội dung kiểm tra</th>
                <th style="width: 112px;">Kết quả</th>
                <th style="width: 176px;">Ghi chú</th>
              </tr>
            </thead>
            <tbody>${rows}</tbody>
          </table>

          <h2 class="section-title">IV. Ý kiến xác nhận</h2>
          <section class="opinion-grid">
            <div class="opinion-box">
              <strong>Khoa Dinh dưỡng / Bộ phận phụ trách</strong>
              ${escapeHtml(report.yKienKhoaDinhDuong || report.yKienBoPhanPhuTrach || '-')}
            </div>
            <div class="opinion-box">
              <strong>Bộ phận chế biến và cung cấp suất ăn</strong>
              ${escapeHtml(report.yKienBoPhanCheBien || report.yKienBPCB || '-')}
            </div>
          </section>

          <div class="signatures">
            ${renderSignature(employeeProfile, 'Nhân viên khảo sát', `Ngày ký: ${formatDateTime(employeeProfile?.updatedAt || report.submittedAt)}`)}
            ${renderSignature(adminProfile, 'Admin duyệt biên bản', `Ngày duyệt: ${formatDateTime(approvedAt || adminProfile?.updatedAt)}`)}
          </div>

          <footer class="footer">
            <span>Biên bản được tạo từ hệ thống Khảo Sát Bếp.</span>
            <span>${escapeHtml(report.soBienBan || report.id)} - ${escapeHtml(formatDateTime(exportedAt))}</span>
          </footer>
        </main>
        <script>window.onload = () => { window.print() }<\/script>
      </body>
    </html>
  `)
  printWindow.document.close()
  await updateReport(report.id, { exportedAt })
  await loadReports()
  showToast('Đã mở bản in PDF chuẩn hồ sơ.')
}

onMounted(async () => {
  document.body.classList.add('admin-page-scroll')
  startAdminIntro()
  await loadReports()
  employeeSignature.value = getSignatureProfile('employee')
  adminSignature.value = getSignatureProfile('admin')
  if (adminSignature.value) {
    adminName.value = adminSignature.value.name || adminName.value
    adminRole.value = adminSignature.value.role || adminRole.value
  }

  await nextTick()
  setupCanvas()
  updateMobileBackTop()
  window.addEventListener('scroll', updateMobileBackTop, { passive: true })
  window.addEventListener('resize', updateMobileBackTop)
  window.addEventListener('ksb-reports-updated', loadReports)
})

onUnmounted(() => {
  document.body.classList.remove('admin-page-scroll')
  window.clearTimeout(toastTimer)
  window.clearInterval(introProgressTimer)
  window.clearTimeout(introFinishTimer)
  window.removeEventListener('scroll', updateMobileBackTop)
  window.removeEventListener('resize', updateMobileBackTop)
  window.removeEventListener('ksb-reports-updated', loadReports)
})

watch(activeSection, async section => {
  if (section === 'reports') {
    employeeSignature.value = getSignatureProfile('employee')
    adminSignature.value = getSignatureProfile('admin')
    if (adminSignature.value) {
      adminName.value = adminSignature.value.name || adminName.value
      adminRole.value = adminSignature.value.role || adminRole.value
    }
    await nextTick()
    setupCanvas()
  }
})
</script>

<template>
  <div class="admin-layout" :class="{ 'intro-finished': !isAdminIntroLoading }">
    <AppToast :visible="toast.visible" :message="toast.message" />
    <button
      v-show="showMobileBackTop"
      type="button"
      class="mobile-back-top"
      aria-label="Quay lại đầu trang"
      @click="scrollToAdminTop"
    >
      <ion-icon name="chevron-up-outline"></ion-icon>
    </button>

    <Transition name="admin-intro">
      <div v-if="isAdminIntroLoading" class="admin-intro-loader" aria-live="polite">
        <div class="admin-intro-card">
          <div class="admin-intro-logo">
            <img src="../assets/logo.png" alt="Logo" />
          </div>
          <span>Bảng điều khiển quản trị</span>
          <strong>Đang tải tổng quan</strong>
          <div class="admin-intro-track">
            <div class="admin-intro-fill" :style="{ width: `${adminIntroProgress}%` }"></div>
          </div>
        </div>
      </div>
    </Transition>

    <aside class="admin-sidebar">
      <div class="brand">
        <img src="../assets/logo.png" alt="Logo" />
        <div>
          <h2>Admin</h2>
          <p>Duyệt biên bản bếp</p>
        </div>
      </div>

      <nav class="admin-nav" aria-label="Điều hướng admin">
        <button
          type="button"
          class="admin-nav-btn"
          :class="{ active: activeSection === 'dashboard' }"
          @click="openSection('dashboard')"
        >
          <ion-icon name="grid-outline"></ion-icon>
          <span>Tổng quan</span>
        </button>
        <button
          type="button"
          class="admin-nav-btn"
          :class="{ active: activeSection === 'reports' }"
          @click="openSection('reports')"
        >
          <ion-icon name="document-text-outline"></ion-icon>
          <span>Biên bản cần duyệt</span>
        </button>
        <button
          type="button"
          class="admin-nav-btn"
          :class="{ active: activeSection === 'reportList' }"
          @click="openSection('reportList')"
        >
          <ion-icon name="albums-outline"></ion-icon>
          <span>Danh sách biên bản</span>
        </button>

        <label
          class="template-combo"
          :class="{ active: activeSection === 'templateManager' }"
        >
          <span>
            <ion-icon name="create-outline"></ion-icon>
            Quản lý biên bản
          </span>
          <select v-model="selectedTemplateType" @change="openTemplateManager(selectedTemplateType)">
            <option disabled value="">Chọn mẫu đơn</option>
            <option v-for="(label, type) in reportTypeLabels" :key="type" :value="type">
              {{ label }}
            </option>
          </select>
        </label>

        <button
          type="button"
          class="admin-nav-btn"
          :class="{ active: activeSection === 'signature' }"
          @click="openSection('signature')"
        >
          <ion-icon name="brush-outline"></ion-icon>
          <span>Chữ ký điện tử</span>
        </button>
      </nav>

      <div class="notification-area">
        <button
          type="button"
          class="notification-card"
          :class="{ active: activeSection === 'reportList' }"
          @click="openSection('reportList')"
        >
          <ion-icon name="notifications-outline"></ion-icon>
          <div>
            <strong>{{ unreadCount }} thông báo mới</strong>
            <span>Biên bản nhân viên vừa gửi</span>
          </div>
        </button>
      </div>

      <button type="button" class="logout-btn" @click="logout">
        <ion-icon name="log-out-outline"></ion-icon>
        Đăng xuất
      </button>
    </aside>

    <main ref="adminMainRef" class="admin-main">
      <header class="admin-header">
        <div>
          <span class="eyebrow">Bảng điều khiển quản trị</span>
          <h1>Quản lý biên bản kiểm tra</h1>
          <p>Xem, chỉnh sửa, duyệt, ký điện tử và xuất PDF biên bản nhân viên gửi lên.</p>
        </div>
      </header>

      <SignaturePadPage
        v-if="activeSection === 'signature'"
        storage-role="admin"
        default-name="Admin duyệt biên bản"
        default-role="Quản lý Khoa Dinh Dưỡng"
        success-message="Đã lưu chữ ký admin thành công!"
        note-text="Chữ ký này sẽ là chữ ký duyệt của admin. Khi xuất PDF, hệ thống sẽ ghép cùng chữ ký nhân viên khảo sát."
      />

      <template v-else-if="activeSection === 'dashboard'">
        <section class="dashboard-grid dashboard-reveal dashboard-reveal-1">
          <article
            v-for="(card, index) in dashboardCards"
            :key="card.label"
            class="dashboard-card"
            :style="{ '--reveal-index': index }"
          >
            <div class="dashboard-card-icon">
              <ion-icon :name="card.icon"></ion-icon>
            </div>
            <div>
              <span>{{ card.label }}</span>
              <strong>{{ card.value }}</strong>
              <p>{{ card.note }}</p>
            </div>
          </article>
        </section>

        <section class="dashboard-panels dashboard-reveal dashboard-reveal-2">
          <article class="analytics-panel module-panel dashboard-panel-card">
            <div class="panel-head">
              <div>
                <h2>Biên bản theo module</h2>
                <p>Phân bố số biên bản nhân viên đã gửi theo từng nhóm kiểm tra.</p>
              </div>
            </div>

            <div class="module-bars">
              <div v-for="stat in typeStats" :key="stat.type" class="module-bar-row">
                <div class="module-bar-head">
                  <strong>{{ stat.label }}</strong>
                  <span>{{ stat.count }} biên bản</span>
                </div>
                <div class="module-bar-track">
                  <div class="module-bar-fill" :style="{ width: `${(stat.count / maxModuleCount) * 100}%` }"></div>
                </div>
              </div>
            </div>
          </article>

          <article class="analytics-panel status-panel dashboard-panel-card">
            <div class="panel-head">
              <div>
                <h2>Trạng thái xử lý</h2>
                <p>Tổng hợp tiến độ duyệt để ưu tiên xử lý trong ngày.</p>
              </div>
            </div>

            <div class="status-bars">
              <div v-for="stat in statusStats" :key="stat.label" class="status-bar-item">
                <div class="status-bar-column">
                  <div
                    class="status-bar-fill"
                    :style="{ height: `${(stat.value / maxStatusCount) * 100}%`, background: stat.color }"
                  ></div>
                </div>
                <strong>{{ stat.value }}</strong>
                <span>{{ stat.label }}</span>
              </div>
            </div>
          </article>
        </section>

        <section class="analytics-panel recent-panel dashboard-reveal dashboard-reveal-3">
          <div class="panel-head">
            <div>
              <h2>Biên bản gần đây</h2>
              <p>Chọn một biên bản để mở chi tiết, chỉnh sửa, duyệt và xuất PDF.</p>
            </div>
          </div>

          <div v-if="recentReports.length" class="recent-list">
            <button
              v-for="report in recentReports"
              :key="report.id"
              type="button"
              class="recent-item"
              @click="selectReport(report)"
            >
              <div>
                <strong>{{ report.soBienBan }}</strong>
                <span>{{ report.label }} - {{ report.time }}</span>
              </div>
              <small>{{ report.status }}</small>
            </button>
          </div>

          <div v-else class="dashboard-empty">
            <ion-icon name="file-tray-outline"></ion-icon>
            <p>Chưa có dữ liệu biên bản để thống kê.</p>
          </div>
        </section>
      </template>

      <template v-else-if="activeSection === 'reportList'">
        <section class="report-library-panel">
          <div class="report-library-head">
            <div>
              <h2>Danh sách biên bản</h2>
              <p>Tra cứu, lọc theo thời gian và mở chi tiết từng biên bản theo nhóm nghiệp vụ.</p>
            </div>
            <div class="report-total-pill">{{ filteredReports.length }}/{{ reports.length }} biên bản</div>
          </div>

          <div class="report-filters">
            <label class="filter-field search-field">
              <span>Tìm kiếm</span>
              <div>
                <ion-icon name="search-outline"></ion-icon>
                <input v-model="reportSearch" type="text" placeholder="Nhập mã biên bản, loại, trạng thái..." />
              </div>
            </label>

            <label class="filter-field">
              <span>Từ ngày</span>
              <input v-model="reportDateFrom" type="date" />
            </label>

            <label class="filter-field">
              <span>Đến ngày</span>
              <input v-model="reportDateTo" type="date" />
            </label>

            <button type="button" class="btn secondary filter-reset" @click="resetReportFilters">
              Xóa lọc
            </button>
          </div>
        </section>

        <section class="report-columns">
          <article v-for="group in reportListGroups" :key="group.type" class="report-column">
            <div class="report-column-head">
              <div>
                <span>{{ group.label }}</span>
                <strong>{{ group.reports.length }}</strong>
              </div>
              <ion-icon name="folder-open-outline"></ion-icon>
            </div>

            <div v-if="group.reports.length" class="report-column-list">
              <button
                v-for="report in group.reports"
                :key="report.id"
                type="button"
                class="report-library-item"
                :class="{ unread: !report.readByAdmin, active: report.id === selectedId }"
                @click="selectReport(report)"
              >
                <div class="report-library-topline">
                  <strong>{{ report.soBienBan }}</strong>
                  <span>{{ report.status }}</span>
                </div>
                <small>{{ new Date(report.submittedAt || report.updatedAt).toLocaleString('vi-VN') }}</small>
                <p>Ngày kiểm tra: {{ report.ngayKiemTra }}</p>
              </button>
            </div>

            <div v-else class="report-column-empty">
              <ion-icon name="file-tray-outline"></ion-icon>
              <span>Không có biên bản phù hợp.</span>
            </div>
          </article>
        </section>
      </template>

      <template v-else-if="activeSection === 'templateManager'">
        <section class="template-form-shell">
          <div class="template-admin-actions">
            <div>
              <span class="eyebrow">Quản lý mẫu biên bản</span>
              <h2>{{ reportTypeLabels[selectedTemplateType] }}</h2>
              <p>Chỉnh sửa nội dung tiêu chí. Nhân viên mở form sau khi cập nhật sẽ thấy mẫu mới.</p>
            </div>
            <button
              type="button"
              class="btn primary"
              :disabled="!templateEditorDetails.length"
              @click="updateTemplate"
            >
              Cập nhật đơn
            </button>
          </div>

          <div v-if="templateEditorDetails.length" class="template-form-preview">
            <div class="template-glass-card template-header-card">
              <h2>{{ templateMeta.title }}</h2>
              <p>{{ templateMeta.subtitle }}</p>
            </div>

            <div class="template-glass-card">
              <h3>Thông tin chung</h3>
              <div class="template-info-grid">
                <label>
                  <span>Ngày kiểm tra</span>
                  <input type="date" disabled />
                </label>
                <label>
                  <span>Trạng thái mẫu</span>
                  <input type="text" value="Đang chỉnh sửa mẫu" disabled />
                </label>
              </div>
            </div>

            <div class="template-glass-card">
              <div class="template-section-heading">
                <div>
                  <h3>Nội dung kiểm tra</h3>
                  <p>{{ templateEditorDetails.length }} tiêu chí trong mẫu {{ reportTypeLabels[selectedTemplateType] }}</p>
                </div>
              </div>

              <div v-for="section in templateSections" :key="section.title" class="template-section-block">
                <div class="template-section-title">
                  <h4>{{ section.title }}</h4>
                  <button type="button" class="btn secondary" @click="addTemplateDetail(section.title)">
                    Thêm tiêu chí
                  </button>
                </div>

                <div class="template-form-table-wrap">
                  <table class="template-form-table">
                    <thead>
                      <tr>
                        <th>TT</th>
                        <th>Nội dung kiểm tra</th>
                        <th>Đạt</th>
                        <th>K.Đạt</th>
                        <th>Ghi chú</th>
                        <th></th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="detail in section.items" :key="`${detail.phanNhom}-${detail.mucSo}-${detail.index}`">
                        <td>
                          <input v-model="templateEditorDetails[detail.index].mucSo" type="number" min="1" class="template-order-input" />
                        </td>
                        <td>
                          <textarea v-model="templateEditorDetails[detail.index].noiDung" rows="3"></textarea>
                        </td>
                        <td class="text-center"><input type="radio" disabled /></td>
                        <td class="text-center"><input type="radio" disabled /></td>
                        <td><input type="text" disabled placeholder="Nhân viên nhập khi khảo sát" /></td>
                        <td>
                          <button type="button" class="template-delete-btn" @click="removeTemplateDetail(detail.index)">
                            <ion-icon name="trash-outline"></ion-icon>
                          </button>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>

            <div class="template-glass-card">
              <h3>Ý kiến xác nhận</h3>
              <div class="template-info-grid">
                <label>
                  <span>Khoa/Bộ phận phụ trách</span>
                  <textarea disabled rows="3" placeholder="Nhân viên nhập khi khảo sát"></textarea>
                </label>
                <label>
                  <span>Bộ phận chế biến và cung cấp suất ăn</span>
                  <textarea disabled rows="3" placeholder="Nhân viên nhập khi khảo sát"></textarea>
                </label>
              </div>
            </div>
          </div>

          <div v-else class="dashboard-empty">
            <ion-icon name="file-tray-outline"></ion-icon>
            <p>Chưa có dữ liệu mẫu cho loại biên bản này. Hãy gửi ít nhất một biên bản mẫu từ nhân viên trước.</p>
          </div>
        </section>
      </template>

      <template v-else>
        <section v-if="activeReport" ref="selectedReportSectionRef" class="workspace-grid selected-workspace">
        <div class="editor-panel">
          <div class="panel-head">
            <div>
              <h2>{{ activeReport.soBienBan }}</h2>
              <p>{{ reportTypeLabels[activeReport.loaiBienBan] || activeReport.loaiBienBan }} - {{ activeReport.ngayKiemTra }}</p>
            </div>
            <div class="report-head-actions">
              <div class="status-pill">{{ activeReport.status }}</div>
            </div>
          </div>

          <div class="single-chart">
            <div>
              <strong>{{ selectedStats.datPercent }}%</strong>
              <span>Đạt</span>
            </div>
            <div>
              <strong>{{ selectedStats.khongDatPercent }}%</strong>
              <span>Không đạt</span>
            </div>
          </div>

          <div class="table-wrap">
            <table class="review-table">
              <thead>
                <tr>
                  <th>TT</th>
                  <th>Nội dung</th>
                  <th>Đạt</th>
                  <th>K.Đạt</th>
                  <th>Ghi chú</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="detail in activeReport.chiTiets" :key="`${detail.phanNhom}-${detail.mucSo}`">
                  <td>{{ detail.mucSo }}</td>
                  <td>
                    <small>{{ detail.phanNhom }}</small>
                    <textarea v-model="detail.noiDung" rows="2" :readonly="!isEditingReport"></textarea>
                  </td>
                  <td><input v-model="detail.dat" type="radio" :name="`admin_${detail.mucSo}_${detail.phanNhom}`" :value="true" disabled /></td>
                  <td><input v-model="detail.dat" type="radio" :name="`admin_${detail.mucSo}_${detail.phanNhom}`" :value="false" disabled /></td>
                  <td><input v-model="detail.ghiChu" type="text" readonly /></td>
                </tr>
              </tbody>
            </table>
          </div>

          <div class="form-signatures">
            <div class="form-signature-box">
              <strong>Nhân viên khảo sát</strong>
              <div v-if="employeeSignature?.imageData" class="form-signature-image">
                <img :src="employeeSignature.imageData" alt="Chữ ký nhân viên" />
              </div>
              <div v-else class="form-signature-missing">Chưa có chữ ký</div>
              <b>{{ employeeSignature?.name || 'Nhân viên khảo sát' }}</b>
              <span>{{ employeeSignature?.role || 'Khối kiểm tra bếp' }}</span>
            </div>

            <div class="form-signature-box">
              <strong>Admin duyệt</strong>
              <div v-if="adminSignature?.imageData" class="form-signature-image">
                <img :src="adminSignature.imageData" alt="Chữ ký admin" />
              </div>
              <div v-else class="form-signature-missing">Chưa có chữ ký</div>
              <b>{{ adminSignature?.name || adminName }}</b>
              <span>{{ adminSignature?.role || adminRole }}</span>
            </div>
          </div>

          <div class="action-row">
            <button v-if="activeReport.status !== 'approved'" type="button" class="btn approve" @click="approveReport">Duyệt biên bản</button>
            <button type="button" class="btn primary" @click="exportPdfAdvanced">Xuất PDF</button>
          </div>
        </div>
        </section>

        <section v-else class="empty-state">
          <ion-icon name="document-text-outline"></ion-icon>
          <p>{{ reports.length ? 'Chọn một biên bản ở danh sách thông báo bên trái để xem chi tiết.' : 'Chưa có biên bản nào. Hãy gửi một biên bản từ tài khoản nhân viên để admin nhận thông báo.' }}</p>
        </section>

        <section class="summary-grid">
          <div class="summary-card">
            <span>Tổng biên bản</span>
            <strong>{{ totalSubmitted }}</strong>
          </div>
          <div class="summary-card">
            <span>Chờ xử lý</span>
            <strong>{{ pendingCount }}</strong>
          </div>
          <div class="summary-card">
            <span>Đã duyệt</span>
            <strong>{{ approvedCount }}</strong>
          </div>
          <div class="summary-card">
            <span>Thông báo mới</span>
            <strong>{{ unreadCount }}</strong>
          </div>
        </section>

        <section class="analytics-panel">
          <div class="panel-head">
            <h2>Thống kê % đạt / không đạt</h2>
          </div>
          <div class="chart-list">
            <div v-for="stat in typeStats" :key="stat.type" class="chart-row">
              <div class="chart-label">
                <strong>{{ stat.label }}</strong>
                <span>{{ stat.count }} biên bản</span>
              </div>
              <div class="bar-track">
                <div class="bar-pass" :style="{ width: `${stat.datPercent}%` }">{{ stat.datPercent }}%</div>
                <div class="bar-fail" :style="{ width: `${stat.khongDatPercent}%` }">{{ stat.khongDatPercent }}%</div>
              </div>
            </div>
          </div>
        </section>

      </template>
    </main>
  </div>
</template>

<style scoped>
:global(body.admin-page-scroll) {
  overflow: auto;
}

.admin-layout {
  position: relative;
  display: grid;
  grid-template-columns: 300px minmax(0, 1fr);
  min-height: 100dvh;
  background: #eef5fb;
  color: #0f172a;
}

.mobile-back-top {
  display: none;
}

.admin-intro-loader {
  position: fixed;
  inset: 0;
  z-index: 60;
  display: grid;
  place-items: center;
  background:
    radial-gradient(circle at 24% 18%, rgba(14, 165, 233, 0.22), transparent 30%),
    linear-gradient(135deg, rgba(234, 244, 251, 0.96), rgba(248, 250, 252, 0.98));
  backdrop-filter: blur(8px);
}

.admin-intro-card {
  display: grid;
  justify-items: center;
  width: min(360px, calc(100vw - 42px));
  padding: 30px;
  border: 1px solid rgba(186, 230, 253, 0.9);
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.86);
  box-shadow: 0 26px 70px rgba(15, 23, 42, 0.16);
  animation: introCardIn 0.7s cubic-bezier(0.2, 0.8, 0.2, 1) both;
}

.admin-intro-logo {
  position: relative;
  display: grid;
  place-items: center;
  width: 76px;
  height: 76px;
  margin-bottom: 18px;
  border-radius: 22px;
  background: #e0f2fe;
}

.admin-intro-logo::before {
  content: '';
  position: absolute;
  inset: -10px;
  border: 2px solid rgba(14, 165, 233, 0.28);
  border-top-color: #0ea5e9;
  border-radius: 28px;
  animation: introSpin 1.25s linear infinite;
}

.admin-intro-logo img {
  width: 48px;
  height: 48px;
  object-fit: contain;
}

.admin-intro-card span {
  color: #0284c7;
  font-size: 0.78rem;
  font-weight: 900;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.admin-intro-card strong {
  margin: 8px 0 20px;
  color: #0f172a;
  font-size: 1.35rem;
}

.admin-intro-track {
  width: 100%;
  height: 9px;
  overflow: hidden;
  border-radius: 999px;
  background: #e2e8f0;
}

.admin-intro-fill {
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, #0ea5e9, #22c55e);
  transition: width 0.18s ease;
}

.admin-intro-enter-active,
.admin-intro-leave-active {
  transition: opacity 0.42s ease;
}

.admin-intro-enter-from,
.admin-intro-leave-to {
  opacity: 0;
}

.admin-intro-enter-active .admin-intro-card,
.admin-intro-leave-active .admin-intro-card {
  transition: transform 0.42s ease, opacity 0.42s ease;
}

.admin-intro-enter-from .admin-intro-card,
.admin-intro-leave-to .admin-intro-card {
  opacity: 0;
  transform: translateY(18px) scale(0.97);
}

@keyframes introSpin {
  to {
    transform: rotate(360deg);
  }
}

@keyframes introCardIn {
  from {
    opacity: 0;
    transform: translateY(22px) scale(0.96);
  }

  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

.admin-sidebar {
  display: flex;
  flex-direction: column;
  gap: 18px;
  height: 100dvh;
  padding: 22px;
  background: #ffffff;
  border-right: 1px solid #dbe4ef;
  overflow: hidden;
}

.brand {
  display: flex;
  align-items: center;
  gap: 12px;
}

.brand img {
  width: 42px;
  height: 42px;
  object-fit: contain;
}

.brand h2,
.brand p {
  margin: 0;
}

.brand p {
  color: #64748b;
  font-size: 0.9rem;
}

.notification-card {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
  padding: 14px;
  border: 1px solid transparent;
  border-radius: 12px;
  background: #eff6ff;
  color: #075985;
  font: inherit;
  text-align: left;
  cursor: pointer;
  transition: border-color 0.2s ease, background 0.2s ease, transform 0.2s ease;
}

.notification-card:hover {
  transform: translateY(-1px);
  border-color: #bae6fd;
}

.notification-card.active {
  border-color: #38bdf8;
  background: #e0f2fe;
}

.notification-card ion-icon {
  font-size: 1.5rem;
}

.notification-card span {
  display: block;
  margin-top: 4px;
  color: #0369a1;
  font-size: 0.85rem;
}

.notification-area {
  display: grid;
  grid-template-rows: auto minmax(0, auto);
  gap: 12px;
  min-height: 0;
}

.admin-nav {
  display: grid;
  gap: 8px;
}

.admin-nav-btn {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  min-height: 44px;
  padding: 10px 12px;
  border: 1px solid transparent;
  border-radius: 10px;
  background: transparent;
  color: #334155;
  font: inherit;
  font-weight: 800;
  text-align: left;
  cursor: pointer;
  transition: background 0.2s ease, border-color 0.2s ease, color 0.2s ease;
}

.admin-nav-btn:hover {
  background: #f8fafc;
  color: #075985;
}

.admin-nav-btn ion-icon {
  color: #0284c7;
  font-size: 1.15rem;
}

.admin-nav-btn.active {
  border-color: #7dd3fc;
  background: #e0f2fe;
  color: #075985;
}

.template-combo {
  display: grid;
  gap: 8px;
  width: 100%;
  min-height: 64px;
  padding: 10px 12px;
  border: 1px solid transparent;
  border-radius: 10px;
  background: transparent;
  color: #334155;
  font-weight: 800;
}

.template-combo.active {
  border-color: #7dd3fc;
  background: #e0f2fe;
  color: #075985;
}

.template-combo > span {
  display: inline-flex;
  align-items: center;
  gap: 10px;
}

.template-combo ion-icon {
  color: #0284c7;
  font-size: 1.15rem;
}

.template-combo select {
  appearance: none;
  -webkit-appearance: none;
  width: 100%;
  min-height: 40px;
  border: 1px solid #b7d7ea;
  border-radius: 11px;
  padding: 8px 38px 8px 12px;
  background:
    url("data:image/svg+xml,%3Csvg width='16' height='16' viewBox='0 0 20 20' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath d='M5.5 7.5L10 12l4.5-4.5' fill='none' stroke='%230284c7' stroke-width='2.4' stroke-linecap='round' stroke-linejoin='round'/%3E%3C/svg%3E") calc(100% - 14px) 50% / 16px 16px no-repeat,
    linear-gradient(180deg, #ffffff, #f8fbff);
  color: #0f172a;
  font: inherit;
  font-weight: 800;
  outline: none;
  box-shadow: 0 8px 18px rgba(14, 165, 233, 0.08);
  cursor: pointer;
  transition: border-color 0.2s ease, box-shadow 0.2s ease, background 0.2s ease;
}

.template-combo select:hover {
  border-color: #38bdf8;
  background:
    url("data:image/svg+xml,%3Csvg width='16' height='16' viewBox='0 0 20 20' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath d='M5.5 7.5L10 12l4.5-4.5' fill='none' stroke='%230369a1' stroke-width='2.4' stroke-linecap='round' stroke-linejoin='round'/%3E%3C/svg%3E") calc(100% - 14px) 50% / 16px 16px no-repeat,
    linear-gradient(180deg, #ffffff, #eff6ff);
}

.template-combo select:focus {
  border-color: #0284c7;
  box-shadow: 0 0 0 3px rgba(14, 165, 233, 0.18), 0 10px 22px rgba(14, 165, 233, 0.12);
}

.template-combo select option {
  color: #0f172a;
  background: #ffffff;
  font-weight: 700;
}

.template-combo select option:disabled {
  color: #94a3b8;
}

.logout-btn {
  margin-top: auto;
  display: flex;
  justify-content: center;
  gap: 8px;
  padding: 12px;
  border: 1px solid #fecaca;
  border-radius: 10px;
  background: #fff1f2;
  color: #dc2626;
  font-weight: 700;
  cursor: pointer;
}

.admin-main {
  min-width: 0;
  height: 100dvh;
  padding: 26px;
  overflow-y: auto;
}

.intro-finished .admin-sidebar {
  animation: adminSidebarEnter 0.72s cubic-bezier(0.2, 0.8, 0.2, 1) both;
}

.intro-finished .admin-header {
  animation: adminHeaderEnter 0.72s 0.08s cubic-bezier(0.2, 0.8, 0.2, 1) both;
}

.dashboard-reveal {
  animation: dashboardSectionEnter 0.7s cubic-bezier(0.2, 0.8, 0.2, 1) both;
}

.dashboard-reveal-1 {
  animation-delay: 0.18s;
}

.dashboard-reveal-2 {
  animation-delay: 0.34s;
}

.dashboard-reveal-3 {
  animation-delay: 0.5s;
}

.dashboard-grid .dashboard-card {
  animation: dashboardCardEnter 0.68s cubic-bezier(0.2, 0.8, 0.2, 1) both;
  animation-delay: calc(0.18s + (var(--reveal-index, 0) * 0.08s));
}

.dashboard-panel-card {
  animation: dashboardCardEnter 0.72s cubic-bezier(0.2, 0.8, 0.2, 1) both;
}

.module-bar-fill,
.status-bar-fill {
  transform-origin: left bottom;
}

.intro-finished .module-bar-fill {
  animation: moduleFillEnter 0.95s 0.62s cubic-bezier(0.2, 0.8, 0.2, 1) both;
}

.intro-finished .status-bar-fill {
  animation: statusFillEnter 0.9s 0.68s cubic-bezier(0.2, 0.8, 0.2, 1) both;
}

@keyframes adminSidebarEnter {
  from {
    opacity: 0;
    transform: translateX(-18px);
  }

  to {
    opacity: 1;
    transform: translateX(0);
  }
}

@keyframes adminHeaderEnter {
  from {
    opacity: 0;
    transform: translateY(-14px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes dashboardSectionEnter {
  from {
    opacity: 0;
    transform: translateY(22px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes dashboardCardEnter {
  from {
    opacity: 0;
    transform: translateY(18px) scale(0.98);
  }

  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

@keyframes moduleFillEnter {
  from {
    transform: scaleX(0.04);
  }

  to {
    transform: scaleX(1);
  }
}

@keyframes statusFillEnter {
  from {
    transform: scaleY(0.08);
  }

  to {
    transform: scaleY(1);
  }
}

.admin-header {
  margin-bottom: 18px;
}

.eyebrow {
  color: #0284c7;
  font-size: 0.78rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.1em;
}

.admin-header h1 {
  margin: 8px 0;
  font-size: 2rem;
}

.admin-header p {
  margin: 0;
  color: #64748b;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
  margin-bottom: 16px;
}

.dashboard-card {
  display: flex;
  align-items: center;
  gap: 14px;
  min-height: 120px;
  padding: 18px;
  border: 1px solid #dbe4ef;
  border-radius: 12px;
  background: #ffffff;
  box-shadow: 0 12px 28px rgba(15, 23, 42, 0.06);
}

.dashboard-card-icon {
  display: grid;
  place-items: center;
  width: 52px;
  height: 52px;
  border-radius: 14px;
  background: #e0f2fe;
  color: #0284c7;
  font-size: 1.5rem;
  flex: 0 0 auto;
}

.dashboard-card span,
.dashboard-card p {
  color: #64748b;
}

.dashboard-card strong {
  display: block;
  margin: 6px 0;
  color: #0f172a;
  font-size: 2rem;
  line-height: 1;
}

.dashboard-card p {
  margin: 0;
  font-size: 0.85rem;
}

.dashboard-panels {
  display: grid;
  grid-template-columns: minmax(0, 1.35fr) minmax(320px, 0.85fr);
  gap: 16px;
  margin-bottom: 16px;
}

.module-bars {
  display: grid;
  gap: 16px;
}

.module-bar-row {
  display: grid;
  gap: 8px;
}

.module-bar-head {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  color: #0f172a;
}

.module-bar-head span {
  color: #64748b;
  font-size: 0.88rem;
}

.module-bar-track {
  height: 18px;
  overflow: hidden;
  border-radius: 999px;
  background: #e2e8f0;
}

.module-bar-fill {
  height: 100%;
  min-width: 10px;
  border-radius: inherit;
  background: linear-gradient(90deg, #0ea5e9, #22c55e);
}

.status-bars {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 14px;
  align-items: end;
  min-height: 220px;
  padding-top: 10px;
}

.status-bar-item {
  display: grid;
  justify-items: center;
  gap: 8px;
  text-align: center;
}

.status-bar-column {
  display: flex;
  align-items: flex-end;
  width: 58px;
  height: 140px;
  padding: 6px;
  border-radius: 14px;
  background: #f1f5f9;
}

.status-bar-fill {
  width: 100%;
  min-height: 8px;
  border-radius: 10px;
}

.status-bar-item strong {
  color: #0f172a;
  font-size: 1.3rem;
}

.status-bar-item span {
  color: #64748b;
  font-size: 0.86rem;
  font-weight: 700;
}

.recent-panel {
  margin-bottom: 16px;
}

.recent-list {
  display: grid;
  gap: 10px;
}

.recent-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  width: 100%;
  padding: 14px;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  background: #f8fafc;
  color: #0f172a;
  font: inherit;
  text-align: left;
  cursor: pointer;
}

.recent-item:hover {
  border-color: #7dd3fc;
  background: #f0f9ff;
}

.recent-item strong,
.recent-item span {
  display: block;
}

.recent-item span,
.recent-item small {
  color: #64748b;
}

.recent-item small {
  padding: 6px 10px;
  border-radius: 999px;
  background: #e0f2fe;
  color: #075985;
  font-weight: 800;
  white-space: nowrap;
}

.dashboard-empty {
  display: grid;
  place-items: center;
  gap: 10px;
  min-height: 180px;
  border: 1px dashed #cbd5e1;
  border-radius: 12px;
  background: #f8fafc;
  color: #64748b;
  text-align: center;
}

.dashboard-empty ion-icon {
  color: #94a3b8;
  font-size: 2rem;
}

.report-library-panel {
  padding: 18px;
  margin-bottom: 16px;
  border: 1px solid #dbe4ef;
  border-radius: 12px;
  background: #ffffff;
  box-shadow: 0 12px 28px rgba(15, 23, 42, 0.06);
}

.report-library-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 16px;
}

.report-library-head h2,
.report-library-head p {
  margin: 0;
}

.report-library-head p {
  margin-top: 6px;
  color: #64748b;
}

.report-total-pill {
  padding: 8px 12px;
  border-radius: 999px;
  background: #e0f2fe;
  color: #075985;
  font-weight: 800;
  white-space: nowrap;
}

.report-filters {
  display: grid;
  grid-template-columns: minmax(260px, 1fr) 180px 180px auto;
  gap: 12px;
  align-items: end;
}

.filter-field {
  display: grid;
  gap: 7px;
}

.filter-field span {
  color: #475569;
  font-size: 0.86rem;
  font-weight: 800;
}

.filter-field input,
.search-field div {
  width: 100%;
  min-height: 42px;
  border: 1px solid #cbd5e1;
  border-radius: 10px;
  background: #f8fafc;
}

.filter-field input {
  padding: 9px 12px;
  color: #0f172a;
  font: inherit;
}

.search-field div {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 12px;
}

.search-field ion-icon {
  color: #64748b;
}

.search-field input {
  min-height: 40px;
  padding: 0;
  border: none;
  outline: none;
  background: transparent;
}

.filter-reset {
  min-height: 42px;
}

.report-columns {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 14px;
}

.report-column {
  display: flex;
  flex-direction: column;
  min-height: 430px;
  padding: 14px;
  border: 1px solid #dbe4ef;
  border-radius: 12px;
  background: #ffffff;
  box-shadow: 0 12px 28px rgba(15, 23, 42, 0.05);
}

.report-column-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding-bottom: 12px;
  border-bottom: 1px solid #e2e8f0;
}

.report-column-head span,
.report-column-head strong {
  display: block;
}

.report-column-head span {
  color: #475569;
  font-size: 0.9rem;
  font-weight: 800;
}

.report-column-head strong {
  margin-top: 4px;
  color: #0f172a;
  font-size: 1.5rem;
}

.report-column-head ion-icon {
  color: #0284c7;
  font-size: 1.45rem;
}

.report-column-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  min-height: 0;
  max-height: 520px;
  margin-top: 12px;
  padding-right: 4px;
  overflow-y: auto;
}

.report-column-list::-webkit-scrollbar {
  width: 6px;
}

.report-column-list::-webkit-scrollbar-thumb {
  border-radius: 999px;
  background: #cbd5e1;
}

.report-library-item {
  position: relative;
  display: grid;
  gap: 8px;
  width: 100%;
  padding: 12px;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  background: #f8fafc;
  color: #0f172a;
  font: inherit;
  text-align: left;
  cursor: pointer;
}

.report-library-item:hover,
.report-library-item.active {
  border-color: #38bdf8;
  background: #eff6ff;
}

.report-library-item.unread::after {
  content: '';
  position: absolute;
  top: 12px;
  right: 12px;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #ef4444;
}

.report-library-topline {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding-right: 10px;
}

.report-library-topline strong {
  font-size: 0.95rem;
}

.report-library-topline span {
  padding: 4px 8px;
  border-radius: 999px;
  background: #e0f2fe;
  color: #075985;
  font-size: 0.72rem;
  font-weight: 800;
}

.report-library-item small,
.report-library-item p {
  color: #64748b;
  font-size: 0.82rem;
}

.report-library-item p {
  margin: 0;
}

.report-column-empty {
  display: grid;
  place-items: center;
  gap: 8px;
  min-height: 220px;
  margin-top: 12px;
  border: 1px dashed #cbd5e1;
  border-radius: 10px;
  background: #f8fafc;
  color: #64748b;
  text-align: center;
}

.report-column-empty ion-icon {
  color: #94a3b8;
  font-size: 1.8rem;
}

.template-manager-panel {
  padding: 18px;
  border: 1px solid #dbe4ef;
  border-radius: 12px;
  background: #ffffff;
  box-shadow: 0 12px 28px rgba(15, 23, 42, 0.06);
}

.template-manager-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 16px;
}

.template-manager-head h2,
.template-manager-head p {
  margin: 0;
}

.template-manager-head p {
  margin-top: 6px;
  color: #64748b;
}

.template-table-wrap {
  max-height: calc(100dvh - 260px);
  overflow: auto;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
}

.template-table {
  width: 100%;
  border-collapse: collapse;
  table-layout: fixed;
  font-size: 0.9rem;
}

.template-table th,
.template-table td {
  padding: 13px 12px;
  border-bottom: 1px solid #e2e8f0;
  vertical-align: top;
}

.template-table th {
  position: sticky;
  top: 0;
  z-index: 1;
  background: #f8fafc;
  color: #334155;
  text-align: left;
}

.template-table th:nth-child(1),
.template-table td:nth-child(1) {
  width: 72px;
  text-align: center;
}

.template-table th:nth-child(2),
.template-table td:nth-child(2) {
  width: 220px;
}

.template-table textarea {
  width: 100%;
  min-height: 76px;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  padding: 9px 10px;
  color: #0f172a;
  background: #f8fafc;
  font: inherit;
  resize: vertical;
}

.template-form-shell {
  display: flex;
  flex-direction: column;
  gap: 18px;
  max-width: 1240px;
  margin: 0 auto;
}

.template-admin-actions,
.template-glass-card {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  box-shadow: 0 12px 28px rgba(15, 23, 42, 0.06);
}

.template-admin-actions {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  padding: 18px;
}

.template-admin-actions h2,
.template-admin-actions p {
  margin: 0;
}

.template-admin-actions p {
  margin-top: 6px;
  color: #64748b;
}

.template-form-preview {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.template-glass-card {
  padding: 22px;
}

.template-header-card {
  text-align: center;
  background: linear-gradient(135deg, #e0f2fe, #dbeafe);
  border-bottom: 3px solid #0284c7;
}

.template-header-card h2 {
  margin: 0 0 8px;
  color: #0f172a;
  font-size: 1.7rem;
}

.template-header-card p {
  margin: 0;
  color: #475569;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
}

.template-glass-card h3 {
  margin: 0 0 16px;
  padding-bottom: 10px;
  border-bottom: 1px solid #e2e8f0;
  color: #0f172a;
}

.template-info-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
}

.template-info-grid label {
  display: grid;
  gap: 8px;
}

.template-info-grid span {
  color: #475569;
  font-weight: 800;
}

.template-info-grid input,
.template-info-grid textarea {
  width: 100%;
  border: 1px solid #cbd5e1;
  border-radius: 10px;
  padding: 10px 12px;
  color: #64748b;
  background: #f8fafc;
  font: inherit;
}

.template-section-heading {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.template-section-heading h3,
.template-section-heading p {
  margin: 0;
}

.template-section-heading p {
  margin-top: 6px;
  color: #64748b;
}

.template-section-block {
  margin-top: 16px;
}

.template-section-title {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 10px;
}

.template-section-title h4 {
  margin: 0;
  color: #0f172a;
}

.template-form-table-wrap {
  overflow: auto;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
}

.template-form-table {
  width: 100%;
  border-collapse: collapse;
  table-layout: fixed;
  font-size: 0.9rem;
}

.template-form-table th,
.template-form-table td {
  padding: 12px;
  border-bottom: 1px solid #e2e8f0;
  vertical-align: middle;
}

.template-form-table th {
  background: #f8fafc;
  color: #334155;
  text-align: center;
}

.template-form-table th:nth-child(1),
.template-form-table td:nth-child(1) {
  width: 78px;
  text-align: center;
}

.template-form-table th:nth-child(3),
.template-form-table td:nth-child(3),
.template-form-table th:nth-child(4),
.template-form-table td:nth-child(4) {
  width: 86px;
  text-align: center;
}

.template-form-table th:nth-child(5),
.template-form-table td:nth-child(5) {
  width: 210px;
}

.template-form-table th:nth-child(6),
.template-form-table td:nth-child(6) {
  width: 58px;
  text-align: center;
}

.template-form-table textarea,
.template-form-table input[type='text'],
.template-order-input {
  width: 100%;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  padding: 9px 10px;
  color: #0f172a;
  background: #f8fafc;
  font: inherit;
}

.template-form-table textarea {
  min-height: 76px;
  resize: vertical;
}

.template-form-table input:disabled {
  color: #94a3b8;
  background: #f8fafc;
}

.template-delete-btn {
  display: inline-grid;
  place-items: center;
  width: 36px;
  height: 36px;
  border: 1px solid #fecaca;
  border-radius: 9px;
  background: #fff1f2;
  color: #dc2626;
  cursor: pointer;
}

.text-center {
  text-align: center;
}

.summary-grid,
.workspace-grid {
  display: grid;
  gap: 16px;
}

.summary-grid {
  grid-template-columns: repeat(4, minmax(0, 1fr));
  margin: 18px 0 16px;
}

.summary-card,
.analytics-panel,
.editor-panel,
.signature-panel,
.empty-state {
  background: #ffffff;
  border: 1px solid #dbe4ef;
  border-radius: 12px;
  box-shadow: 0 12px 28px rgba(15, 23, 42, 0.06);
}

.summary-card {
  padding: 18px;
}

.summary-card span {
  color: #64748b;
}

.summary-card strong {
  display: block;
  margin-top: 8px;
  font-size: 2rem;
}

.analytics-panel,
.editor-panel,
.signature-panel {
  padding: 18px;
}

.panel-head {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 14px;
}

.panel-head h2,
.panel-head p {
  margin: 0;
}

.panel-head p {
  margin-top: 6px;
  color: #64748b;
}

.chart-list {
  display: grid;
  gap: 12px;
}

.chart-row {
  display: grid;
  grid-template-columns: 190px minmax(0, 1fr);
  gap: 14px;
  align-items: center;
}

.chart-label span {
  display: block;
  color: #64748b;
  font-size: 0.85rem;
}

.bar-track {
  display: flex;
  height: 30px;
  overflow: hidden;
  border-radius: 999px;
  background: #e2e8f0;
}

.bar-pass,
.bar-fail {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 36px;
  color: #ffffff;
  font-size: 0.78rem;
  font-weight: 800;
}

.bar-pass {
  background: #16a34a;
}

.bar-fail {
  background: #dc2626;
}

.workspace-grid {
  grid-template-columns: minmax(0, 1fr);
  margin: 16px 0 22px;
}

.selected-workspace {
  margin-top: 0;
  margin-bottom: 22px;
}

.status-pill {
  align-self: flex-start;
  padding: 7px 10px;
  border-radius: 999px;
  background: #f0f9ff;
  color: #0369a1;
  font-weight: 700;
}

.report-head-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 10px;
  flex-wrap: wrap;
}

.single-chart {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px;
  margin-bottom: 14px;
}

.single-chart div {
  padding: 14px;
  border-radius: 10px;
  background: #f8fafc;
}

.single-chart strong {
  display: block;
  font-size: 1.6rem;
}

.single-chart span {
  color: #64748b;
}

.table-wrap {
  max-height: 560px;
  overflow: auto;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  background: #ffffff;
}

.review-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.86rem;
  table-layout: fixed;
}

.review-table th:nth-child(1),
.review-table td:nth-child(1) {
  width: 64px;
  text-align: center;
}

.review-table th:nth-child(2),
.review-table td:nth-child(2) {
  width: auto;
}

.review-table th:nth-child(3),
.review-table td:nth-child(3),
.review-table th:nth-child(4),
.review-table td:nth-child(4) {
  width: 88px;
  text-align: center;
}

.review-table th:nth-child(5),
.review-table td:nth-child(5) {
  width: 220px;
}

.review-table th,
.review-table td {
  padding: 14px 12px;
  border-bottom: 1px solid #e2e8f0;
  vertical-align: middle;
}

.review-table th {
  position: sticky;
  top: 0;
  background: #f8fafc;
  z-index: 1;
  text-align: center;
  white-space: nowrap;
}

.review-table small {
  display: block;
  margin-bottom: 6px;
  color: #0284c7;
  font-weight: 700;
}

.review-table textarea,
.review-table input[type='text'],
.signature-panel input {
  width: 100%;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  padding: 8px 10px;
  font: inherit;
  color: #0f172a;
  background: #f8fafc;
}

.review-table textarea {
  min-height: 58px;
  resize: vertical;
}

.review-table input[type='radio'] {
  appearance: none;
  -webkit-appearance: none;
  display: block;
  width: 20px;
  height: 20px;
  margin: 0 auto;
  border: 2px solid #cbd5e1;
  border-radius: 50%;
  background: #ffffff;
  box-shadow: inset 0 0 0 4px #ffffff;
  cursor: pointer;
}

.review-table input[type='radio']:checked {
  border-color: #0284c7;
  background: #0284c7;
  box-shadow: inset 0 0 0 4px #ffffff, 0 0 0 3px rgba(2, 132, 199, 0.12);
}

.review-table textarea[readonly],
.review-table input[readonly] {
  color: #334155;
  background: #ffffff;
  border-color: transparent;
  cursor: default;
}

.review-table input[type='radio']:disabled {
  cursor: default;
  opacity: 1;
}

.form-signatures {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 18px;
  margin-top: 18px;
  padding-top: 18px;
  border-top: 1px solid #e2e8f0;
}

.form-signature-box {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  min-height: 170px;
  padding: 16px;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  background: #f8fafc;
  text-align: center;
}

.form-signature-box strong {
  color: #0f172a;
}

.form-signature-image,
.form-signature-missing {
  display: grid;
  place-items: center;
  width: 100%;
  max-width: 240px;
  height: 92px;
  border-bottom: 1px solid #cbd5e1;
}

.form-signature-image img {
  width: 100%;
  height: 86px;
  object-fit: contain;
}

.form-signature-missing {
  color: #64748b;
  font-size: 0.9rem;
}

.form-signature-box b,
.form-signature-box span {
  display: block;
}

.form-signature-box span {
  color: #64748b;
  font-size: 0.9rem;
}

.action-row,
.signature-actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 18px;
  padding-top: 16px;
  border-top: 1px solid #e2e8f0;
}

.btn {
  min-height: 42px;
  padding: 10px 14px;
  border: 1px solid transparent;
  border-radius: 9px;
  font-weight: 800;
  cursor: pointer;
}

.btn.primary {
  background: #0ea5e9;
  color: #ffffff;
}

.template-admin-actions .btn.primary,
.action-row .btn.primary {
  font-family: "Times New Roman", Times, serif;
}

.btn.secondary {
  background: #f8fafc;
  color: #334155;
  border-color: #cbd5e1;
}

.btn.approve {
  background: #16a34a;
  color: #ffffff;
}

.signature-panel {
  align-self: start;
}

.signature-panel h2 {
  margin: 0 0 14px;
}

.signature-panel label {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-bottom: 12px;
  color: #475569;
  font-weight: 700;
}

.signature-canvas {
  display: block;
  width: 100%;
  height: 180px;
  border: 1px dashed #93c5fd;
  border-radius: 12px;
  background: #ffffff;
  touch-action: none;
  cursor: crosshair;
}

.empty-state {
  display: grid;
  place-items: center;
  gap: 10px;
  min-height: 240px;
  margin: 0 0 22px;
  color: #64748b;
  text-align: center;
}

.empty-state ion-icon {
  font-size: 2.4rem;
  color: #0284c7;
}

@media (max-width: 1100px) {
  .admin-layout,
  .workspace-grid {
    grid-template-columns: 1fr;
  }

  .admin-sidebar {
    height: auto;
    overflow: visible;
  }

  .dashboard-grid,
  .report-columns,
  .form-signatures,
  .summary-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .report-filters {
    grid-template-columns: 1fr 1fr;
  }

  .dashboard-panels {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 720px) {
  :global(html),
  :global(body),
  :global(#app) {
    height: auto;
    min-height: 100dvh;
  }

  :global(body.admin-page-scroll) {
    overflow-x: hidden;
    overflow-y: auto;
    -webkit-overflow-scrolling: touch;
  }

  .admin-layout {
    display: block;
    min-height: 100dvh;
  }

  .mobile-back-top {
    position: fixed;
    right: 14px;
    bottom: 16px;
    z-index: 45;
    display: grid;
    place-items: center;
    width: 44px;
    height: 44px;
    border: 1px solid #7dd3fc;
    border-radius: 999px;
    background: #0ea5e9;
    color: #ffffff;
    box-shadow: 0 14px 30px rgba(14, 165, 233, 0.32);
    font-size: 1.25rem;
    cursor: pointer;
  }

  .admin-sidebar {
    position: relative;
    gap: 12px;
    padding: 16px;
    border-right: none;
    border-bottom: 1px solid #dbe4ef;
  }

  .brand {
    justify-content: center;
  }

  .brand img {
    width: 38px;
    height: 38px;
  }

  .brand h2 {
    font-size: 1.35rem;
  }

  .brand p {
    font-size: 0.82rem;
  }

  .admin-nav {
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 8px;
  }

  .admin-nav-btn {
    min-height: 42px;
    padding: 9px 10px;
    border-radius: 11px;
    font-size: 0.92rem;
  }

  .template-combo {
    grid-column: 1 / -1;
    min-height: auto;
    padding: 10px;
  }

  .template-combo select {
    min-height: 42px;
    font-size: 0.94rem;
  }

  .notification-area {
    margin-top: 2px;
  }

  .notification-card {
    padding: 12px;
    border-radius: 12px;
  }

  .notification-card strong,
  .notification-card span {
    font-size: 0.9rem;
  }

  .logout-btn {
    margin-top: 0;
    padding: 11px;
  }

  .admin-main {
    height: auto;
    min-height: 100dvh;
    padding: 18px 14px 28px;
    overflow: visible;
  }

  .admin-header {
    margin-bottom: 14px;
  }

  .eyebrow {
    font-size: 0.72rem;
  }

  .admin-header h1 {
    font-size: 1.55rem;
    line-height: 1.14;
  }

  .admin-header p {
    font-size: 0.92rem;
    line-height: 1.45;
  }

  .dashboard-card,
  .analytics-panel,
  .editor-panel,
  .signature-panel,
  .report-library-panel,
  .report-column,
  .template-glass-card,
  .template-admin-actions {
    border-radius: 10px;
  }

  .dashboard-card {
    min-height: auto;
    padding: 14px;
  }

  .dashboard-card strong {
    font-size: 1.6rem;
  }

  .panel-head {
    flex-direction: column;
    gap: 8px;
  }

  .panel-head h2,
  .report-library-head h2,
  .template-admin-actions h2 {
    font-size: 1.25rem;
  }

  .module-bar-head {
    font-size: 0.92rem;
  }

  .status-bars {
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 8px;
  }

  .status-bar-column {
    width: 48px;
    height: 110px;
  }

  .recent-item,
  .report-library-item {
    padding: 11px;
  }

  .recent-item small {
    white-space: normal;
  }

  .dashboard-grid,
  .report-columns,
  .summary-grid,
  .form-signatures,
  .chart-row {
    grid-template-columns: 1fr;
  }

  .report-filters {
    grid-template-columns: 1fr;
  }

  .report-library-head {
    flex-direction: column;
  }

  .template-manager-head {
    flex-direction: column;
  }

  .template-admin-actions,
  .template-section-title {
    flex-direction: column;
    align-items: stretch;
  }

  .template-info-grid {
    grid-template-columns: 1fr;
  }

  .template-form-table-wrap {
    overflow: visible;
    border: none;
    border-radius: 0;
  }

  .template-form-table,
  .template-form-table tbody,
  .template-form-table tr,
  .template-form-table td {
    display: block;
    width: 100%;
  }

  .template-form-table {
    min-width: 0;
    border-collapse: separate;
    border-spacing: 0;
  }

  .template-form-table thead {
    display: none;
  }

  .template-form-table tr {
    position: relative;
    display: grid;
    grid-template-columns: 78px minmax(0, 1fr) 44px;
    gap: 10px;
    margin-bottom: 12px;
    padding: 12px;
    border: 1px solid #dbe4ef;
    border-radius: 12px;
    background: #ffffff;
    box-shadow: 0 10px 22px rgba(15, 23, 42, 0.05);
  }

  .template-form-table td {
    padding: 0;
    border-bottom: none;
  }

  .template-form-table td:nth-child(1) {
    grid-column: 1;
    grid-row: 1;
  }

  .template-form-table td:nth-child(2) {
    grid-column: 2 / 4;
    grid-row: 1;
  }

  .template-form-table td:nth-child(3),
  .template-form-table td:nth-child(4),
  .template-form-table td:nth-child(5) {
    display: none;
  }

  .template-form-table td:nth-child(6) {
    grid-column: 1 / -1;
    grid-row: 2;
    text-align: right;
  }

  .template-order-input {
    min-height: 44px;
    text-align: center;
  }

  .template-form-table textarea {
    min-height: 96px;
    font-size: 0.95rem;
  }

  .template-delete-btn {
    width: 100%;
    min-height: 40px;
  }

  .template-table {
    min-width: 760px;
  }

  .status-bars {
    min-height: 180px;
  }

  .review-table {
    min-width: 760px;
  }
}

@media (max-width: 420px) {
  .admin-sidebar {
    padding: 14px 12px;
  }

  .admin-nav {
    grid-template-columns: 1fr;
  }

  .admin-nav-btn,
  .template-combo {
    width: 100%;
  }

  .notification-card {
    align-items: flex-start;
  }

  .admin-main {
    padding: 16px 10px 26px;
  }

  .admin-header h1 {
    font-size: 1.42rem;
  }

  .analytics-panel,
  .editor-panel,
  .report-library-panel,
  .template-glass-card {
    padding: 14px;
  }

  .dashboard-card {
    gap: 12px;
  }

  .dashboard-card-icon {
    width: 46px;
    height: 46px;
  }

  .report-column {
    min-height: auto;
    padding: 12px;
  }

  .report-column-list {
    max-height: none;
    overflow: visible;
  }

  .summary-card {
    padding: 14px;
  }

  .single-chart {
    grid-template-columns: 1fr;
  }

  .action-row {
    justify-content: stretch;
  }

  .action-row .btn {
    flex: 1 1 100%;
  }

  .table-wrap,
  .template-form-table-wrap,
  .report-library-panel {
    max-width: 100%;
  }
}
</style>
