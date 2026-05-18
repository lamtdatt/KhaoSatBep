import { apiRequest } from '@/utils/apiClient'
import { getSignatureProfile } from '@/utils/signatureStore'

const REPORTS_KEY = 'ksb_submitted_reports'
const REPORT_META_KEY = 'ksb_report_meta'
const EMPLOYEE_APPROVED_READ_KEY = 'ksb_employee_approved_read_ids'
const REPORT_REFRESH_RETRIES = 3
const REPORT_REFRESH_RETRY_DELAY_MS = 900

const FRONTEND_REPORT_TYPES = {
  CSHT: 'CoSoHaTang',
  HoSo: 'HoSo',
  VeSinh: 'VeSinh',
  SuatAn: 'SuatAnNguoiBenh'
}

const BACKEND_REPORT_TYPES = {
  CoSoHaTang: 'CSHT',
  HoSo: 'HoSo',
  VeSinh: 'VeSinh',
  SuatAnNguoiBenh: 'SuatAn'
}

const STATUS_MAP = {
  ChuaGui: 'submitted',
  DaGui: 'submitted',
  DaDuyet: 'approved',
  TuChoi: 'reviewed'
}

const parseJson = (key, fallback) => {
  if (typeof window === 'undefined') {
    return fallback
  }

  try {
    const raw = window.localStorage.getItem(key)
    return raw ? JSON.parse(raw) : fallback
  } catch (error) {
    console.error(`Khong the doc du lieu tu ${key}:`, error)
    return fallback
  }
}

const writeJson = (key, value) => {
  if (typeof window === 'undefined') {
    return
  }

  window.localStorage.setItem(key, JSON.stringify(value))
}

const readReports = () => parseJson(REPORTS_KEY, [])

const readMeta = () => parseJson(REPORT_META_KEY, {})

const readEmployeeApprovedReadIds = () => parseJson(EMPLOYEE_APPROVED_READ_KEY, [])

const writeReports = reports => {
  writeJson(REPORTS_KEY, reports)
  window.dispatchEvent(new CustomEvent('ksb-reports-updated'))
}

const writeMeta = meta => {
  writeJson(REPORT_META_KEY, meta)
}

const writeEmployeeApprovedReadIds = ids => {
  writeJson(EMPLOYEE_APPROVED_READ_KEY, [...new Set(ids.map(String))])
  window.dispatchEvent(new CustomEvent('ksb-employee-notifications-updated'))
}

const wait = ms => new Promise(resolve => window.setTimeout(resolve, ms))

const mergeMeta = report => {
  const meta = readMeta()[String(report.id)] || {}
  return {
    ...report,
    ...meta
  }
}

const toFrontendType = type => FRONTEND_REPORT_TYPES[type] || type
const toBackendType = type => BACKEND_REPORT_TYPES[type] || type
const toFrontendStatus = status => STATUS_MAP[status] || status || 'submitted'

const normalizeParticipant = item => ({
  stt: item.stt ?? item.STT ?? 0,
  hoTen: item.hoTen ?? item.HoTen ?? '',
  chucVu: item.chucVu ?? item.ChucVu ?? ''
})

const normalizeChecklistItem = item => ({
  id: item.id ?? item.Id ?? null,
  mucSo: item.mucSo ?? item.MucSo ?? 0,
  phanNhom: item.phanNhom ?? item.PhanNhom ?? '',
  noiDung: item.noiDung ?? item.NoiDung ?? '',
  dat: item.dat ?? item.Dat ?? null,
  ghiChu: item.ghiChu ?? item.GhiChu ?? ''
})

const normalizeMealItem = item => ({
  mucSo: item.stt ?? item.STT ?? 0,
  phanNhom: item.loaiSuatAn === 'OngThong' ? 'II. Suất ăn qua ống thông' : 'I. Suất ăn đường miệng',
  noiDung: item.noiDung ?? item.NoiDung ?? '',
  ghiChuNoiDung: '',
  cheDo1KhoiLuong: item.cheDoAn1KhoiLuong ?? item.CheDoAn1KhoiLuong ?? '',
  cheDo1Dat: item.cheDoAn1Dat ?? item.CheDoAn1Dat ?? null,
  cheDo2KhoiLuong: item.cheDoAn2KhoiLuong ?? item.CheDoAn2KhoiLuong ?? '',
  cheDo2Dat: item.cheDoAn2Dat ?? item.CheDoAn2Dat ?? null,
  ghiChu: ''
})

const normalizeSignature = item => ({
  viTri: item.viTri ?? item.ViTri ?? '',
  tenNguoiKy: item.tenNguoiKy ?? item.TenNguoiKy ?? '',
  chucVuNguoiKy: item.chucVuNguoiKy ?? item.ChucVuNguoiKy ?? '',
  duLieuChuKy: item.duLieuChuKy ?? item.DuLieuChuKy ?? '',
  ngayKy: item.ngayKy ?? item.NgayKy ?? ''
})

const normalizeSummary = item => {
  const report = {
    id: item.id ?? item.Id,
    soBienBan: item.soBienBan ?? item.SoBienBan ?? '',
    loaiBienBan: toFrontendType(item.loaiBienBan ?? item.LoaiBienBan ?? ''),
    backendLoaiBienBan: item.loaiBienBan ?? item.LoaiBienBan ?? '',
    ngayKiemTra: item.ngayKiemTra ?? item.NgayKiemTra ?? '',
    status: toFrontendStatus(item.trangThai ?? item.TrangThai ?? ''),
    backendTrangThai: item.trangThai ?? item.TrangThai ?? '',
    nguoiTao: item.nguoiTao ?? item.NguoiTao ?? '',
    submittedAt: item.ngayTao ?? item.NgayTao ?? '',
    updatedAt: item.ngayTao ?? item.NgayTao ?? '',
    soMucDat: item.soMucDat ?? item.SoMucDat ?? 0,
    soMucKhongDat: item.soMucKhongDat ?? item.SoMucKhongDat ?? 0,
    tongSoMuc: item.tongSoMuc ?? item.TongSoMuc ?? 0
  }

  return mergeMeta(report)
}

const normalizeDetail = item => {
  const detailItems = (item.chiTiets ?? item.ChiTiets ?? []).map(normalizeChecklistItem)
  const mealItems = (item.dinhLuongs ?? item.DinhLuongs ?? []).map(normalizeMealItem)
  const report = {
    id: item.id ?? item.Id,
    soBienBan: item.soBienBan ?? item.SoBienBan ?? '',
    loaiBienBan: toFrontendType(item.loaiBienBan ?? item.LoaiBienBan ?? ''),
    backendLoaiBienBan: item.loaiBienBan ?? item.LoaiBienBan ?? '',
    ngayKiemTra: item.ngayKiemTra ?? item.NgayKiemTra ?? '',
    status: toFrontendStatus(item.trangThai ?? item.TrangThai ?? ''),
    backendTrangThai: item.trangThai ?? item.TrangThai ?? '',
    nguoiTao: item.nguoiTao ?? item.NguoiTao ?? '',
    submittedAt: item.ngayTao ?? item.NgayTao ?? '',
    updatedAt: item.ngayTao ?? item.NgayTao ?? '',
    gopYKhoaDinhDuong: item.gopYKhoaDinhDuong ?? item.GopYKhoaDinhDuong ?? '',
    yKienBPCB: item.yKienBPCB ?? item.YKienBPCB ?? '',
    yKienKhoaDinhDuong: item.gopYKhoaDinhDuong ?? item.GopYKhoaDinhDuong ?? '',
    yKienBoPhanPhuTrach: item.gopYKhoaDinhDuong ?? item.GopYKhoaDinhDuong ?? '',
    yKienBoPhanCheBien: item.yKienBPCB ?? item.YKienBPCB ?? '',
    buaAn: (item.buaAnDuongMieng ?? item.BuaAnDuongMieng ?? '')
      .split(',')
      .map(value => value.trim())
      .filter(Boolean),
    buaAnOngThong: (item.buaAnOngThong ?? item.BuaAnOngThong ?? '')
      .split(',')
      .map(value => value.trim())
      .filter(Boolean),
    thucDonThayDoi: (item.thucDonHangNgay ?? item.ThucDonHangNgay ?? '') === 'ThayDoi',
    thanhPhans: (item.thanhPhans ?? item.ThanhPhans ?? []).map(normalizeParticipant),
    chiTiets: [...mealItems, ...detailItems].sort((a, b) => (a.mucSo || 0) - (b.mucSo || 0)),
    chuKys: (item.chuKys ?? item.ChuKys ?? []).map(normalizeSignature),
    dinhLuongs: item.dinhLuongs ?? item.DinhLuongs ?? []
  }

  const stats = getReportStats(report)
  return mergeMeta({
    ...report,
    soMucDat: stats.dat,
    soMucKhongDat: stats.khongDat,
    tongSoMuc: stats.total
  })
}

const upsertReport = report => {
  const reports = readReports()
  const index = reports.findIndex(item => String(item.id) === String(report.id))
  if (index >= 0) {
    reports.splice(index, 1, {
      ...reports[index],
      ...report
    })
  } else {
    reports.unshift(report)
  }

  writeReports(reports)
  return report
}

const applyMetaPatch = (id, patch) => {
  const meta = readMeta()
  const key = String(id)
  meta[key] = {
    ...(meta[key] || {}),
    ...patch
  }
  writeMeta(meta)

  const reports = readReports().map(report => {
    if (String(report.id) !== key) {
      return report
    }

    return {
      ...report,
      ...meta[key]
    }
  })

  writeReports(reports)
}

const getReportSignatures = report => {
  if (report.chuKys?.length) {
    return report.chuKys
  }

  const employeeSignature = getSignatureProfile('employee')
  if (!employeeSignature?.imageData) {
    return []
  }

  return [
    {
      viTri: 'NhanVienKhaoSat',
      tenNguoiKy: employeeSignature.name || 'Nhan vien khao sat',
      chucVuNguoiKy: employeeSignature.role || 'Nhan vien khao sat',
      duLieuChuKy: employeeSignature.imageData
    }
  ]
}

const createBasePayload = report => ({
  soBienBan: report.soBienBan,
  loaiBienBan: toBackendType(report.loaiBienBan),
  ngayKiemTra: report.ngayKiemTra,
  gopYKhoaDinhDuong: report.gopYKhoaDinhDuong || report.yKienKhoaDinhDuong || report.yKienBoPhanPhuTrach || '',
  yKienBPCB: report.yKienBPCB || report.yKienBoPhanCheBien || '',
  thanhPhans: (report.thanhPhans || []).map(item => ({
    stt: item.stt,
    hoTen: item.hoTen,
    chucVu: item.chucVu
  })),
  chuKys: getReportSignatures(report)
})

const buildCreatePayload = report => {
  if (report.loaiBienBan === 'SuatAnNguoiBenh') {
    const mealRows = report.chiTiets || []
    const tubeRows = report.chiTietOngThong || []
    const requirementRows = report.yeuCauRieng || []

    return {
      ...createBasePayload(report),
      buaAnDuongMieng: (report.buaAn || []).join(', '),
      thucDonHangNgay: report.thucDonThayDoi === true ? 'ThayDoi' : 'KhongThayDoi',
      buaAnOngThong: (report.buaAnOngThong || []).join(', '),
      chiTiets: requirementRows.map(item => ({
        mucSo: item.mucSo,
        phanNhom: 'III. Yêu cầu riêng',
        noiDung: `${item.noiDung}${item.moTa ? ` - ${item.moTa}` : ''}`,
        dat: item.dat,
        ghiChu: item.ghiChu || ''
      })),
      dinhLuongs: [
        ...mealRows.map(item => ({
          stt: item.mucSo,
          loaiSuatAn: 'DuongMieng',
          noiDung: item.noiDung,
          cheDoAn1Ten: 'Chế độ ăn 1',
          cheDoAn1KhoiLuong: item.cheDo1KhoiLuong ? Number(item.cheDo1KhoiLuong) : null,
          cheDoAn1Dat: item.cheDo1Dat,
          cheDoAn1KhongDat: item.cheDo1Dat === false,
          cheDoAn2Ten: 'Chế độ ăn 2',
          cheDoAn2KhoiLuong: item.cheDo2KhoiLuong ? Number(item.cheDo2KhoiLuong) : null,
          cheDoAn2Dat: item.cheDo2Dat,
          cheDoAn2KhongDat: item.cheDo2Dat === false
        })),
        ...tubeRows.map(item => ({
          stt: item.mucSo,
          loaiSuatAn: 'OngThong',
          noiDung: item.noiDung,
          cheDoAn1Ten: 'Chế độ ăn 1',
          cheDoAn1KhoiLuong: item.cheDo1KhoiLuong ? Number(item.cheDo1KhoiLuong) : null,
          cheDoAn1Dat: item.cheDo1Dat,
          cheDoAn1KhongDat: item.cheDo1Dat === false,
          cheDoAn2Ten: 'Chế độ ăn 2',
          cheDoAn2KhoiLuong: item.cheDo2KhoiLuong ? Number(item.cheDo2KhoiLuong) : null,
          cheDoAn2Dat: item.cheDo2Dat,
          cheDoAn2KhongDat: item.cheDo2Dat === false
        }))
      ]
    }
  }

  return {
    ...createBasePayload(report),
    chiTiets: (report.chiTiets || []).map(item => ({
      mucSo: item.mucSo,
      phanNhom: item.phanNhom || '',
      noiDung: item.noiDung,
      dat: item.dat,
      ghiChu: item.ghiChu || ''
    }))
  }
}

export const getReports = () => readReports()

const fetchReportSummaries = async () => {
  let lastError = null

  for (let attempt = 1; attempt <= REPORT_REFRESH_RETRIES; attempt += 1) {
    try {
      const response = await apiRequest('/BienBan')
      if (Array.isArray(response) && (response.length || attempt === REPORT_REFRESH_RETRIES)) {
        return response
      }

      if (!Array.isArray(response)) {
        throw new Error('Du lieu bien ban tra ve khong dung dinh dang.')
      }

      await wait(REPORT_REFRESH_RETRY_DELAY_MS * attempt)
    } catch (error) {
      lastError = error
      if (attempt < REPORT_REFRESH_RETRIES) {
        await wait(REPORT_REFRESH_RETRY_DELAY_MS * attempt)
      }
    }
  }

  throw lastError
}

export const refreshReports = async () => {
  const response = await fetchReportSummaries()
  const existing = readReports()

  if (!response.length && existing.length) {
    return existing
  }

  const detailedById = new Map(
    existing
      .filter(report => Array.isArray(report.chiTiets) && report.chiTiets.length)
      .map(report => [String(report.id), report])
  )

  const reports = response.map(item => {
    const normalized = normalizeSummary(item)
    return {
      ...normalized,
      ...(detailedById.get(String(normalized.id)) || {})
    }
  })

  writeReports(reports)
  return reports
}

export const getReportById = async id => {
  const response = await apiRequest(`/BienBan/${id}`)
  const report = normalizeDetail(response)
  upsertReport(report)
  return report
}

export const saveReport = async report => {
  const payload = buildCreatePayload(report)
  const created = await apiRequest('/BienBan', {
    method: 'POST',
    body: JSON.stringify(payload)
  })

  await apiRequest(`/BienBan/${created.id}/gui`, {
    method: 'PATCH'
  })

  const detail = await getReportById(created.id)
  await refreshReports()
  return detail
}

export const updateReport = async (id, patch) => {
  if (patch.status === 'approved') {
    await apiRequest(`/BienBan/${id}/duyet`, {
      method: 'PATCH',
      body: JSON.stringify({
        trangThai: 'DaDuyet',
        ghiChu: patch.adminNote || ''
      })
    })

    const report = await getReportById(id)
    return report
  }

  if (patch.readByAdmin !== undefined || patch.exportedAt !== undefined || patch.approvedAt !== undefined) {
    applyMetaPatch(id, patch)
    return getReports().find(report => String(report.id) === String(id)) || null
  }

  return getReports().find(report => String(report.id) === String(id)) || null
}

export const markReportRead = async id => {
  applyMetaPatch(id, { readByAdmin: true })
  return getReports().find(report => String(report.id) === String(id)) || null
}

export const getUnreadApprovedReports = (reports = readReports()) => {
  const readIds = new Set(readEmployeeApprovedReadIds().map(String))
  return reports.filter(report => report.status === 'approved' && !readIds.has(String(report.id)))
}

export const markApprovedReportsSeen = (reports = readReports()) => {
  const currentReadIds = readEmployeeApprovedReadIds().map(String)
  const approvedIds = reports
    .filter(report => report.status === 'approved')
    .map(report => String(report.id))

  writeEmployeeApprovedReadIds([...currentReadIds, ...approvedIds])
}

export const getReportStats = report => {
  if (!report) {
    return {
      dat: 0,
      khongDat: 0,
      total: 0,
      datPercent: 0,
      khongDatPercent: 0
    }
  }

  const details = Array.isArray(report.chiTiets) ? report.chiTiets : []
  const values = details.flatMap(item => {
    return [item.dat, item.cheDo1Dat, item.cheDo2Dat].filter(value => value === true || value === false)
  })

  const fallbackTotal = report.tongSoMuc ?? report.TongSoMuc ?? 0
  const fallbackDat = report.soMucDat ?? report.SoMucDat ?? 0
  const fallbackKhongDat = report.soMucKhongDat ?? report.SoMucKhongDat ?? 0

  const dat = values.length ? values.filter(value => value === true).length : fallbackDat
  const khongDat = values.length ? values.filter(value => value === false).length : fallbackKhongDat
  const total = values.length || fallbackTotal

  return {
    dat,
    khongDat,
    total,
    datPercent: total ? Math.round((dat / total) * 100) : 0,
    khongDatPercent: total ? Math.round((khongDat / total) * 100) : 0
  }
}

export { REPORTS_KEY }
