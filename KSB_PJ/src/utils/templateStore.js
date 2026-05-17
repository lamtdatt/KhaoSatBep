const TEMPLATE_KEY = 'ksb_report_templates'

const makeDetailKey = detail => `${detail.phanNhom || ''}::${detail.mucSo}`

const item = (mucSo, noiDung, phanNhom = '') => ({
  key: makeDetailKey({ mucSo, phanNhom }),
  mucSo,
  phanNhom,
  noiDung
})

const makeTemplate = (reportType, items) => ({
  reportType,
  items,
  updatedAt: null,
  isDefault: true
})

const DEFAULT_TEMPLATES = {
  CoSoHaTang: makeTemplate('CoSoHaTang', [
    item(1, 'Bếp bố trí theo nguyên tắc 1 chiều, tách biệt nguồn ô nhiễm'),
    item(2, 'Tường, trần nhà, sàn nhà không thấm nước, rạn nứt, ẩm mốc'),
    item(3, 'Hệ thống cung cấp nước'),
    item(4, 'Hệ thống xử lý chất thải'),
    item(5, 'Hệ thống bếp gas, bình gas'),
    item(6, 'Hệ thống điện'),
    item(7, 'Phòng cháy chữa cháy'),
    item(8, 'Máy móc trang thiết bị được bảo trì, bảo dưỡng định kỳ'),
    item(9, 'Thiết bị phòng chống côn trùng'),
    item(10, 'Bồn rửa tay'),
    item(11, 'Bàn ghế'),
    item(12, 'Phương tiện vận chuyển thức ăn'),
    item(13, 'Phương tiện thu gom')
  ]),
  HoSo: makeTemplate('HoSo', [
    item(1, 'Giấy phép kinh doanh'),
    item(2, 'Giấy chứng nhận cơ sở đủ điều kiện An toàn thực phẩm'),
    item(3, 'Giấy xác nhận tập huấn kiến thức về ATVSTP'),
    item(4, 'Giấy xác nhận đủ sức khỏe của người trực tiếp sản xuất, kinh doanh thực phẩm'),
    item(5, 'Hợp đồng mua bán'),
    item(6, 'Hóa đơn mua hàng'),
    item(7, 'Danh sách nhân viên đang làm việc tại bếp có xác nhận của công ty'),
    item(8, 'Bảng mô tả công việc cho nhân viên từng vị trí làm việc'),
    item(9, 'Sổ kiểm thực 03 bước, sổ lưu mẫu thức ăn'),
    item(10, 'Bảng giá niêm yết'),
    item(11, 'Thực đơn/Kế hoạch sản xuất'),
    item(12, 'Bảng định lượng suất ăn'),
    item(13, 'Phân công vệ sinh'),
    item(14, 'Kế hoạch diệt côn trùng'),
    item(15, 'Quy trình hướng dẫn cho từng khu vực'),
    item(16, 'Kế hoạch đào tạo, tập huấn cho nhân viên')
  ]),
  VeSinh: makeTemplate('VeSinh', [
    item(1, 'Nhân viên mặc đồng phục đúng quy định, gọn gàng, sạch sẽ', 'I. Điều kiện về con người'),
    item(2, 'Móng tay cắt ngắn, sạch sẽ, không mang trang sức sai quy định', 'I. Điều kiện về con người'),
    item(3, 'Không hút thuốc, ăn uống trong khu vực chế biến', 'I. Điều kiện về con người'),
    item(4, 'Rửa tay bằng xà phòng trước khi chế biến, sau khi đi vệ sinh và khi chuyển công đoạn', 'I. Điều kiện về con người'),
    item(5, 'Dụng cụ chế biến thực phẩm sống/chín riêng biệt', 'II. Kiểm tra dụng cụ'),
    item(6, 'Dụng cụ chứa đựng thực phẩm sạch sẽ, nguyên vẹn, có nắp đậy', 'II. Kiểm tra dụng cụ'),
    item(7, 'Không gian bếp ăn thông thoáng, sạch sẽ, không tồn đọng nước thải/rác thải', 'III. Giám sát môi trường bếp ăn'),
    item(8, 'Kho thực phẩm được sắp xếp đúng quy định, theo nguyên tắc nhập trước - xuất trước', 'IV. Giám sát kho thực phẩm'),
    item(9, 'Kiểm tra nguồn gốc xuất xứ thực phẩm nhập vào', 'V. Giám sát nguồn nguyên liệu nhập vào'),
    item(10, 'Sơ chế rau, thịt, cá đúng quy trình và đảm bảo vệ sinh', 'VI. Giám sát khâu sơ chế'),
    item(11, 'Chế biến thực phẩm đúng quy trình, thiết bị dụng cụ sạch sẽ', 'VII. Giám sát khâu chế biến'),
    item(12, 'Lưu mẫu thức ăn đúng quy định, đủ nhãn và thời gian lưu', 'VIII. Giám sát lưu mẫu thức ăn'),
    item(13, 'Phân chia suất ăn đúng định lượng, không lẫn dị vật', 'IX. Giám sát phân chia thức ăn'),
    item(14, 'Xử lý dụng cụ phục vụ suất ăn sạch sẽ, đúng quy trình', 'X. Giám sát xử lý dụng cụ')
  ]),
  SuatAnNguoiBenh: makeTemplate('SuatAnNguoiBenh', [
    item(1, 'Cơm'),
    item(2, 'Món mặn'),
    item(3, 'Món chay'),
    item(4, 'Món xào/luộc'),
    item(5, 'Món canh'),
    item(6, 'Tráng miệng'),
    item(7, 'Cháo mặn'),
    item(8, 'Cháo chay'),
    item(9, 'Món nước')
  ])
}

const cloneTemplate = template => {
  if (!template) {
    return null
  }

  return {
    ...template,
    items: template.items.map(item => ({ ...item }))
  }
}

const readTemplates = () => {
  if (typeof window === 'undefined') {
    return {}
  }

  try {
    const raw = window.localStorage.getItem(TEMPLATE_KEY)
    return raw ? JSON.parse(raw) : {}
  } catch (error) {
    console.error('Khong the doc mau bien ban:', error)
    return {}
  }
}

const writeTemplates = templates => {
  if (typeof window === 'undefined') {
    return
  }

  window.localStorage.setItem(TEMPLATE_KEY, JSON.stringify(templates))
  window.dispatchEvent(new CustomEvent('ksb-templates-updated'))
}

export const getReportTemplate = reportType => {
  return readTemplates()[reportType] || null
}

export const getDefaultReportTemplate = reportType => {
  return cloneTemplate(DEFAULT_TEMPLATES[reportType])
}

export const saveReportTemplate = (reportType, details) => {
  const templates = readTemplates()
  const items = (details || []).map(detail => ({
    key: makeDetailKey(detail),
    mucSo: detail.mucSo,
    phanNhom: detail.phanNhom || '',
    noiDung: detail.noiDung || ''
  }))

  templates[reportType] = {
    reportType,
    items,
    updatedAt: new Date().toISOString()
  }

  writeTemplates(templates)
  return templates[reportType]
}

export const applyReportTemplate = (reportType, items, phanNhom = '') => {
  const template = getReportTemplate(reportType)
  if (!template?.items?.length) {
    return items
  }

  const map = new Map(template.items.map(item => [item.key, item]))

  items.forEach(item => {
    const saved = map.get(makeDetailKey({ ...item, phanNhom }))
    if (saved?.noiDung) {
      item.noiDung = saved.noiDung
    }
  })

  return items
}

export { TEMPLATE_KEY }
