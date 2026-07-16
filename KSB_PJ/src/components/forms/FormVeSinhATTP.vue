<script setup>
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue'
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
    { stt: 1, hoTen: '', chucVu: '' }
  ],
  gopYKhoaDinhDuong: '',
  yKienBPCB: ''
})

const group = (id, title, items) => ({ id, title, items })
const item = (mucSo, noiDung, nhomPhu = '') => ({ mucSo, noiDung, nhomPhu, dat: null, ghiChu: '' })

const defaultSections = () => [
  group('I', 'ĐIỀU KIỆN VỀ CON NGƯỜI', [
    item(1, 'Móng tay cắt ngắn, không nấm móng, giữ sạch tay, không đeo đồ trang sức sai quy định'),
    item(2, 'Rửa tay bằng xà phòng trước và sau khi tiếp xúc thực phẩm'),
    item(3, 'Được trang bị đầy đủ và sử dụng bảo hộ lao động'),
    item(4, 'Quần áo, tư trang để đúng nơi quy định'),
    item(5, 'Thái độ làm việc nghiêm túc, không ăn, đùa giỡn trong lúc làm'),
    item(6, 'Phụ trách bộ phận nắm được nguyên tắc một chiều và thực hiện đúng nguyên tắc'),
    item(7, 'Phụ trách bộ phận, nhân viên nắm được kỹ thuật kiểm thực ba bước và có tiến hành kiểm thực ba bước theo đúng kỹ thuật'),
    item(8, 'Phụ trách bộ phận, nhân viên nắm được phương pháp lưu mẫu và thực hiện lưu mẫu đúng nguyên tắc')
  ]),
  group('II', 'KIỂM TRA DỤNG CỤ', [
    item(9, 'Trang thiết bị bảo hộ lao động: quần áo, tạp dề, mũ chụp tóc, giày/ủng, khẩu trang, bao tay... trang bị đầy đủ cho nhân viên'),
    item(10, 'Nhiệt kế theo dõi còn hoạt động và kiểm định đúng hạn'),
    item(11, 'Thùng rác để đúng nơi quy định, có nắp đậy kín, đảm bảo vệ sinh'),
    item(12, 'Dụng cụ: khay ăn, tô, chén, đĩa, muỗng nĩa... đảm bảo cung cấp đầy đủ và đồng bộ, thay thế ngay khi cần'),
    item(13, 'Vật tư tiêu hao: khăn lau, móp lau sàn, khăn lau tay, dung dịch vệ sinh tay... đảm bảo cung cấp đầy đủ, thay thế ngay khi cần'),
    item(14, 'Dụng cụ ăn 1 lần chất liệu thân thiện với môi trường'),
    item(15, 'Dụng cụ: sọt nhựa, dao thớt, rổ, khay đựng thực phẩm... được phân màu theo loại thực phẩm, phân biệt sống chín, sạch sẽ'),
    item(16, 'Khăn lau sử dụng đúng màu theo quy định từng khu vực'),
    item(17, 'Khu vực để dụng cụ bếp được sắp xếp gọn gàng, vệ sinh sạch sẽ'),
    item(18, 'Dụng cụ phục vụ ăn uống sạch sẽ, khô ráo'),
    item(19, 'Dụng cụ che đậy, trang thiết bị bảo quản thức ăn sạch sẽ, đầy đủ')
  ]),
  group('III', 'GIÁM SÁT MÔI TRƯỜNG BẾP ĂN', [
    item(20, 'Không gian bếp ăn thông thoáng, thuận tiện thao tác. Đảm bảo sạch sẽ, không tồn đọng nước thải, rác thải trên sàn, không có côn trùng, động vật gây hại'),
    item(21, 'Đảm bảo nguyên tắc 1 chiều trong môi trường bếp ăn, đảm bảo đầu vào và đầu ra của nguyên liệu/thức ăn ở từng khu vực'),
    item(22, 'Vệ sinh và sắp xếp khu vực làm việc (bồn rửa, bàn, dụng cụ, dao, thớt, bếp nấu)')
  ]),
  group('IV', 'GIÁM SÁT KHO THỰC PHẨM', [
    item(23, 'Thực phẩm được sắp xếp trên giá, kệ đảm bảo khoảng cách theo quy định: cách mặt sàn ít nhất 20cm; cách tường kho ít nhất 30cm; cách trần kho ít nhất 50cm; giữa các kệ có lối đi để dễ kiểm tra', 'GIÁM SÁT KHO THỰC PHẨM KHÔ'),
    item(24, 'Sắp xếp thực phẩm theo nguyên tắc nhập trước - xuất trước, hết hạn trước - sử dụng trước', 'GIÁM SÁT KHO THỰC PHẨM KHÔ'),
    item(25, 'Thực phẩm có nhãn ghi ngày nhập - ngày hết hạn, đảm bảo ngày hết hạn dễ nhìn thấy', 'GIÁM SÁT KHO THỰC PHẨM KHÔ'),
    item(26, 'Theo dõi nhiệt độ và ghi nhận đầy đủ, đúng hạn 2 lần/ngày (ngày giờ - nhiệt độ - ký tên)', 'GIÁM SÁT KHO THỰC PHẨM KHÔ'),
    item(27, 'Kho được vệ sinh sạch sẽ, không mùi ẩm mốc', 'GIÁM SÁT KHO THỰC PHẨM KHÔ'),
    item(28, 'Theo dõi, kiểm tra an toàn bảo quản thực phẩm đúng quy định: kho mát đạt từ 2 - 8°C; kho đông đạt từ (-18) - (-10)°C', 'GIÁM SÁT KHO THỰC PHẨM LẠNH'),
    item(29, 'Theo dõi nhiệt độ, độ ẩm và ghi nhận đầy đủ, đúng hạn 2 lần/ngày (ngày giờ - nhiệt độ - ký tên): trước khi bắt đầu làm việc và kết thúc ca làm việc', 'GIÁM SÁT KHO THỰC PHẨM LẠNH'),
    item(30, 'Khi nhập hàng, các bao bì giấy, carton, gỗ phải được tháo bỏ trước khi lưu. Các loại thực phẩm được đóng gói và che đậy kín đúng quy định', 'GIÁM SÁT KHO THỰC PHẨM LẠNH'),
    item(31, 'Dán tem nhãn, ngày giờ nhập cho thực phẩm lưu trữ trong tủ', 'GIÁM SÁT KHO THỰC PHẨM LẠNH'),
    item(32, 'Thực phẩm trong tủ được sắp xếp đảm bảo khoảng cách theo quy định', 'GIÁM SÁT KHO THỰC PHẨM LẠNH'),
    item(33, 'Sắp xếp thực phẩm đúng khu vực quy định', 'GIÁM SÁT KHO THỰC PHẨM LẠNH'),
    item(34, 'Xuất thực phẩm theo nguyên tắc nhập trước - xuất trước, hết hạn trước - sử dụng trước', 'GIÁM SÁT KHO THỰC PHẨM LẠNH')
  ]),
  group('V', 'GIÁM SÁT NGUỒN NGUYÊN LIỆU NHẬP VÀO', [
    item(35, 'Kiểm tra nguồn gốc xuất xứ thực phẩm nhập với hợp đồng tại bếp: nguồn gốc thực phẩm; người đưa thực phẩm (đối chiếu giấy tờ nguồn gốc thực phẩm)'),
    item(36, 'Thịt - Cảm quan: bề mặt thịt khô, sạch, không dính lông và tạp chất lạ; mùi tự nhiên của thịt, không có mùi ôi, mùi chua; mỡ có màu trắng sữa hoặc trắng hồng, thịt nạc có màu đặc trưng. Trong suốt quá trình vận chuyển, thịt luôn được duy trì nhiệt độ sao cho sản phẩm trong khoảng 0 - 4°C. Hạn sử dụng không quá 7 ngày.', 'KIỂM TRA CHẤT LƯỢNG THỰC PHẨM'),
    item(37, 'Cá/động vật giáp xác có vỏ - Cảm quan: cá có màu sắc sáng, nhớt không bị biến màu, ruột nguyên vẹn, sạch đường tiêu hóa, không có ký sinh trùng, máu đỏ tươi, mang tươi, không thối rữa; động vật giáp xác có màu sắc da sáng, thịt trắng chắc, có độ bám dính vào thịt, mùi tươi; sau khi chế biến: vị tươi, không ôi, chua, hỏng, cấu trúc rắn chắc', 'KIỂM TRA CHẤT LƯỢNG THỰC PHẨM'),
    item(38, 'Rau củ trái cây - Cảm quan: màu tự nhiên, không có mùi ôi thối của rau củ, không bị dập nát hoặc thâm úng, lá không bị sâu hay đốm bã chè, không có mùi thuốc bảo vệ thực vật hay mùi lạ', 'KIỂM TRA CHẤT LƯỢNG THỰC PHẨM')
  ]),
  group('VI', 'GIÁM SÁT KHÂU SƠ CHẾ', [
    item(39, 'Trang phục gọn gàng, sạch sẽ, tuân thủ bảo hộ lao động và mang khẩu trang y tế', 'NHÂN VIÊN SƠ CHẾ'),
    item(40, 'Rửa tay đúng quy trình, móng tay không dài, không bị nhiễm nấm móng, không mang trang sức sai quy định', 'NHÂN VIÊN SƠ CHẾ'),
    item(41, 'Dụng cụ sơ chế được làm bằng vật liệu đảm bảo an toàn, đúng quy định', 'DỤNG CỤ SỬ DỤNG SƠ CHẾ NGUYÊN LIỆU'),
    item(42, 'Dụng cụ được dán nhãn phân biệt, phân loại màu sử dụng riêng cho từng loại nguyên liệu', 'DỤNG CỤ SỬ DỤNG SƠ CHẾ NGUYÊN LIỆU'),
    item(43, 'Bồn, bàn sơ chế sạch sẽ, không bám cặn, chất bẩn trên bề mặt bàn, kệ bàn', 'DỤNG CỤ SỬ DỤNG SƠ CHẾ NGUYÊN LIỆU'),
    item(44, 'Sử dụng khay, rổ đúng màu quy định cho từng loại nguyên liệu, vệ sinh sạch sẽ, không bám cặn, chất bẩn trong kệ khay rổ', 'DỤNG CỤ SỬ DỤNG SƠ CHẾ NGUYÊN LIỆU'),
    item(45, 'Dụng cụ sau khi sử dụng sơ chế được vệ sinh sạch sẽ và bảo quản riêng theo khu vực quy định', 'DỤNG CỤ SỬ DỤNG SƠ CHẾ NGUYÊN LIỆU'),
    item(46, 'Rau được nhặt hết lá dập, úa, loại bỏ phần rau không ăn được. Gọt vỏ, loại bỏ chất bẩn, sâu', 'SƠ CHẾ RAU CỦ QUẢ'),
    item(47, 'Rửa rau theo nguyên tắc: rửa ít nhất 3 lần bằng nước sạch dưới vòi nước chảy hoặc rửa đến khi sạch. Rửa cả tàu lá, củ, miếng to rồi mới cắt nhỏ', 'SƠ CHẾ RAU CỦ QUẢ'),
    item(48, 'Ngâm rau củ quả trong bồn nước muối 1% (pha 100 lít nước hòa tan 1kg muối hột) từ 5 - 10 phút', 'SƠ CHẾ RAU CỦ QUẢ'),
    item(49, 'Vớt rau củ quả -> cắt nhỏ theo yêu cầu -> chuyển sang khu vực sạch chờ chế biến', 'SƠ CHẾ RAU CỦ QUẢ'),
    item(50, 'Đối với trái cây phục vụ tráng miệng, chuyển nguyên liệu sang khu vực chia suất, tiến hành gọt bỏ vỏ kích thước và chia định lượng phù hợp với yêu cầu. Bảo quản sạch chờ phục vụ suất ăn', 'SƠ CHẾ RAU CỦ QUẢ'),
    item(51, 'Cạo sạch lông trên da, loại bỏ máu bầm, thịt thừa. Rửa thịt dưới vòi nước chảy ít nhất 2 lần', 'SƠ CHẾ THỊT'),
    item(52, 'Để thịt vào rổ cho ráo nước. Cắt thái thịt theo đúng định lượng yêu cầu', 'SƠ CHẾ THỊT'),
    item(53, 'Đựng thịt vào thùng nhựa, dán nhãn cho sản phẩm (tên sản phẩm, ngày giờ sơ chế). Cất giữ vào tủ mát chờ chế biến hoặc chuyển qua khu vực chế biến', 'SƠ CHẾ THỊT'),
    item(54, 'Ngâm cá trong bồn rửa với nước muối loãng. Cạo vảy, móc mang, làm sạch ruột, loại bỏ hết máu cá', 'SƠ CHẾ CÁ'),
    item(55, 'Rửa lại cá ít nhất 2 lần', 'SƠ CHẾ CÁ'),
    item(56, 'Để cá vào rổ cho ráo nước. Cắt khúc cá theo yêu cầu', 'SƠ CHẾ CÁ'),
    item(57, 'Đựng cá trong thùng nhựa, dán nhãn cho sản phẩm (tên sản phẩm, ngày giờ sơ chế). Cất giữ vào tủ mát chờ chế biến hoặc chuyển qua khu vực chế biến', 'SƠ CHẾ CÁ')
  ]),
  group('VII', 'GIÁM SÁT KHÂU CHẾ BIẾN', [
    item(58, 'Trang phục gọn gàng, sạch sẽ, tuân thủ bảo hộ lao động và mang khẩu trang y tế', 'NHÂN VIÊN CHẾ BIẾN'),
    item(59, 'Rửa tay đúng quy trình, móng tay không dài, không bị nhiễm nấm móng, không mang trang sức sai quy định', 'NHÂN VIÊN CHẾ BIẾN'),
    item(60, 'Có bảng hướng dẫn sử dụng, vệ sinh thiết bị đầy đủ', 'ĐỐI VỚI TRANG THIẾT BỊ DỤNG CỤ'),
    item(61, 'Có sổ theo dõi định kỳ bảo dưỡng, lịch vệ sinh và được ghi chép đầy đủ', 'ĐỐI VỚI TRANG THIẾT BỊ DỤNG CỤ'),
    item(62, 'Thiết bị được vệ sinh sạch và bảo quản đúng quy định', 'ĐỐI VỚI TRANG THIẾT BỊ DỤNG CỤ'),
    item(63, 'Thiết bị được kiểm tra đảm bảo an toàn trước khi dùng', 'ĐỐI VỚI TRANG THIẾT BỊ DỤNG CỤ'),
    item(64, 'Dụng cụ chế biến được làm bằng vật liệu đảm bảo an toàn, đúng quy định', 'DỤNG CỤ CHẾ BIẾN'),
    item(65, 'Dụng cụ chế biến được xử lý sạch sẽ, không bám dầu mỡ, cặn thức ăn thừa, đóng cặn bẩn', 'DỤNG CỤ CHẾ BIẾN'),
    item(66, 'Dụng cụ chế biến được sắp xếp, bảo quản ở khu vực riêng, dễ quan sát, thuận tiện thao tác chế biến, không bám dầu mỡ và chất phát sinh từ quá trình chế biến', 'DỤNG CỤ CHẾ BIẾN'),
    item(67, 'Dụng cụ chứa đựng được phân biệt, dùng riêng cho thực phẩm chín và sống', 'DỤNG CỤ CHẾ BIẾN'),
    item(68, 'Hộp đựng gia vị được để ở khu vực riêng biệt không bị ảnh hưởng của nhiệt độ', 'DỤNG CỤ CHẾ BIẾN'),
    item(69, 'Có cân điện tử để cân lượng gia vị chính xác theo yêu cầu thực đơn', 'DỤNG CỤ CHẾ BIẾN'),
    item(70, 'Nguyên liệu thực phẩm được kiểm tra chất lượng trước khi chế biến', 'QUY TRÌNH THỰC HIỆN'),
    item(71, 'Có quy định hướng dẫn chế biến món ăn', 'QUY TRÌNH THỰC HIỆN'),
    item(72, 'Có bảng định lượng nguyên liệu và gia vị cho từng chế độ bệnh lý', 'QUY TRÌNH THỰC HIỆN'),
    item(73, 'Thực phẩm đã sơ chế được phân chia riêng theo từng chế độ, không trộn lẫn nguyên liệu giữa các món với nhau', 'QUY TRÌNH THỰC HIỆN'),
    item(74, 'Gia vị món ăn được cân đong chính xác theo yêu cầu bệnh lý', 'QUY TRÌNH THỰC HIỆN'),
    item(75, 'Thực phẩm sau chế biến được chứa đựng trong dụng cụ riêng biệt, có dán nhãn tên chế độ bệnh lý', 'QUY TRÌNH THỰC HIỆN'),
    item(76, 'Thực phẩm sau chế biến được chứa đựng trong tủ giữ nóng, được bao bọc kín, nhiệt độ thức ăn phải được giữ 60 - 65°C và không lẫn với thực phẩm chưa chế biến', 'QUY TRÌNH THỰC HIỆN'),
    item(77, 'Thời gian bảo quản, vận chuyển suất ăn từ khi chế biến xong đến khi ăn không quá 4 giờ; thời gian từ khi vận chuyển suất ăn đến khi ăn không quá 2 giờ', 'QUY TRÌNH THỰC HIỆN'),
    item(78, 'Thức ăn khi phục vụ người bệnh/khách hàng phải đảm bảo còn ấm nóng', 'QUY TRÌNH THỰC HIỆN'),
    item(79, 'Vệ sinh, dọn dẹp môi trường làm việc sau khi chế biến xong', 'QUY TRÌNH THỰC HIỆN')
  ]),
  group('VIII', 'GIÁM SÁT LƯU MẪU', [
    item(80, 'Trang phục gọn gàng, sạch sẽ, tuân thủ bảo hộ lao động và mang khẩu trang y tế, mang găng tay dùng 1 lần', 'NHÂN VIÊN'),
    item(81, 'Rửa tay đúng quy trình, móng tay không dài, không bị nhiễm nấm móng, không mang trang sức sai quy định', 'NHÂN VIÊN'),
    item(82, 'Dụng cụ lưu mẫu thức ăn phải có nắp đậy kín, không có hoa văn và được làm từ vật liệu đảm bảo tránh thôi nhiễm khi tiếp xúc với thực phẩm (dùng thủy tinh hoặc inox)', 'DỤNG CỤ LƯU MẪU'),
    item(83, 'Dụng cụ lấy mẫu: mỗi mẫu sử dụng 1 bộ muỗng, thìa, kẹp riêng', 'DỤNG CỤ LƯU MẪU'),
    item(84, 'Dụng cụ lưu mẫu và lấy mẫu phải được rửa sạch, khử trùng trước khi dùng (khử trùng bằng tủ sấy ở 70°C 40 - 60 phút hoặc chần trong nước sôi từ 3 - 5 phút)', 'DỤNG CỤ LƯU MẪU'),
    item(85, 'Có tủ lưu mẫu riêng biệt với tủ thực phẩm khác', 'DỤNG CỤ LƯU MẪU'),
    item(86, 'Lượng mẫu lưu cần lấy: thức ăn đặc (các món xào, hấp, rán, luộc...); rau, quả ăn ngay (rau sống, quả tráng miệng,...) tối thiểu 100g; thức ăn lỏng (súp, canh...) tối thiểu 150ml', 'QUY TRÌNH LƯU MẪU THỨC ĂN'),
    item(87, 'Thời điểm lưu mẫu được lấy trước khi phân chia suất ăn hoặc trước khi vận chuyển suất ăn', 'QUY TRÌNH LƯU MẪU THỨC ĂN'),
    item(88, 'Mỗi món ăn được lấy và lưu vào dụng cụ lưu mẫu riêng, chỉ mở nắp hộp lưu khi cho mẫu vào và đậy lại ngay sau đó', 'QUY TRÌNH LƯU MẪU THỨC ĂN'),
    item(89, 'Mẫu lưu được dán nhãn mẫu thức ăn lưu với các thông tin: bữa ăn, tên mẫu thức ăn, thời gian lấy, người lấy mẫu, ký tên', 'QUY TRÌNH LƯU MẪU THỨC ĂN'),
    item(90, 'Nhãn mẫu thức ăn được in từ loại giấy mỏng, đảm bảo rách khi niêm phong khi mở nắp', 'QUY TRÌNH LƯU MẪU THỨC ĂN'),
    item(91, 'Để mẫu nguội trong phòng lưu mẫu sau đó đặt vào tủ lưu mẫu', 'QUY TRÌNH LƯU MẪU THỨC ĂN'),
    item(92, 'Nhiệt độ bảo quản mẫu thức ăn lưu từ 2 - 8°C', 'QUY TRÌNH LƯU MẪU THỨC ĂN'),
    item(93, 'Theo dõi, kiểm tra nhiệt độ tủ lưu mẫu ít nhất 2 lần/ngày (trước khi bắt đầu làm việc và sau khi kết thúc ca làm việc) và ghi nhận đầy đủ (ngày giờ - nhiệt độ - ký tên)', 'QUY TRÌNH LƯU MẪU THỨC ĂN'),
    item(94, 'Thời gian lưu mẫu trong 24 giờ kể từ khi lưu', 'QUY TRÌNH LƯU MẪU THỨC ĂN'),
    item(95, 'Sau 24 giờ lưu mẫu không có nghi ngờ ngộ độc thực phẩm hoặc không có yêu cầu của cơ quan quản lý thì tiến hành hủy mẫu lưu', 'QUY TRÌNH LƯU MẪU THỨC ĂN'),
    item(96, 'Ghi sổ lưu và hủy mẫu thức ăn (theo Quyết định số 1246/QĐ-BYT)', 'QUY TRÌNH LƯU MẪU THỨC ĂN')
  ]),
  group('IX', 'GIÁM SÁT PHÂN CHIA THỨC ĂN', [
    item(97, 'Nhân viên chia suất đảm bảo đồng phục, đồ bảo hộ như găng tay dùng 1 lần, mũ trùm tóc, tạp dề', 'NHÂN VIÊN CHIA SUẤT'),
    item(98, 'Nhân viên thực hiện rửa tay trước và sau khi chia suất', 'NHÂN VIÊN CHIA SUẤT'),
    item(99, 'Nhân viên chia suất không tự ý di chuyển ra ngoài khu vực chia suất khi đang làm việc đến những khu vực khác để tránh lây nhiễm, không đảm bảo vệ sinh cho suất ăn. Ngược lại, nhân viên khu vực khác cũng không tự ý di chuyển vào khu vực chia suất', 'NHÂN VIÊN CHIA SUẤT'),
    item(100, 'Khay chén, dụng cụ ăn uống (muỗng, đũa), dụng cụ chia suất (muỗng, vá, đũa...) được làm từ vật liệu đảm bảo an toàn, dễ vệ sinh', 'DỤNG CỤ CHIA SUẤT'),
    item(101, 'Dụng cụ ăn uống, khay chén phải được rửa sạch, phơi khô hoặc hấp nóng, sấy cho khô trước khi chia suất ăn', 'DỤNG CỤ CHIA SUẤT'),
    item(102, 'Có cân điện tử để cân đong khối lượng thức ăn cho các suất ăn bệnh lý có yêu cầu khối lượng loại thực phẩm khác nhau', 'DỤNG CỤ CHIA SUẤT'),
    item(103, 'Các món ăn đã nấu chín đang chờ để được chia suất phải được bảo quản nóng như đựng trong hộp, thùng có đậy kín nắp, chất liệu giữ nhiệt', 'DỤNG CỤ CHIA SUẤT'),
    item(104, 'Khu vực chia suất được phân khu hợp lý, theo dây chuyền phân chia suất ăn', 'THỰC HIỆN PHÂN CHIA SUẤT ĂN'),
    item(105, 'Nhân viên cân đong suất ăn mẫu theo đúng yêu cầu của thực đơn bệnh lý, từ đó những suất ăn tiếp theo cân theo đúng định lượng suất ăn mẫu', 'THỰC HIỆN PHÂN CHIA SUẤT ĂN'),
    item(106, 'Thức ăn trong suất ăn đạt các yêu cầu về cảm quan món ăn, không để bị khét, nguội lạnh, ôi thiu, biến chất hay có lẫn dị vật như tóc, móng, nhựa...', 'THỰC HIỆN PHÂN CHIA SUẤT ĂN'),
    item(107, 'Sau khi chia suất xong cần đậy nắp kín, có tem nhãn thông tin bệnh nhân và thông tin dinh dưỡng suất ăn. Đưa suất ăn vào tủ bảo quản nóng', 'THỰC HIỆN PHÂN CHIA SUẤT ĂN'),
    item(108, 'Thời gian từ khi chia suất xong, vận chuyển suất ăn đến khi ăn không quá 2 giờ', 'THỰC HIỆN PHÂN CHIA SUẤT ĂN'),
    item(109, 'Dọn dẹp, vệ sinh sạch khu vực chia suất sau khi đã hoàn thành công việc', 'THỰC HIỆN PHÂN CHIA SUẤT ĂN')
  ]),
  group('X', 'GIÁM SÁT XỬ LÝ DỤNG CỤ PHỤC VỤ SUẤT ĂN', [
    item(110, 'Trang phục gọn gàng, sạch sẽ, tuân thủ bảo hộ lao động và mang khẩu trang y tế', 'NHÂN VIÊN'),
    item(111, 'Rửa tay đúng quy trình, móng tay không dài, không bị nhiễm nấm móng, không mang trang sức sai quy định', 'NHÂN VIÊN'),
    item(112, 'Bồn rửa vệ sinh sạch, không bám cặn bẩn, không có cặn bã thức ăn trong bồn rửa, lồng hứng và không có rác ứ đọng', 'TRANG THIẾT BỊ DỤNG CỤ'),
    item(113, 'Hệ thống thoát nước thông suốt, không có rác thải dồn ứ, tắc nghẽn và tràn nước ra sàn nhà', 'TRANG THIẾT BỊ DỤNG CỤ'),
    item(114, 'Dụng cụ rửa: miếng rửa chén sạch, nguyên vẹn không nát, dính vào dụng cụ', 'TRANG THIẾT BỊ DỤNG CỤ'),
    item(115, 'Dung dịch rửa dụng cụ nằm trong danh mục cho phép của Bộ Y tế về ATTP và quy định của bệnh viện', 'TRANG THIẾT BỊ DỤNG CỤ'),
    item(116, 'Các rổ, khay chứa dụng cụ sạch sau khi rửa phải đảm bảo vệ sinh, không đóng cặn bẩn, ẩm ướt', 'TRANG THIẾT BỊ DỤNG CỤ'),
    item(117, 'Thiết bị, máy móc có liên quan đến việc xử lý dụng cụ hoạt động tốt, vệ sinh sạch sẽ và có bảng hướng dẫn sử dụng', 'TRANG THIẾT BỊ DỤNG CỤ'),
    item(118, 'Thùng chứa thức ăn thừa có nắp đậy kín, sạch sẽ, không có thức ăn thừa để quá 24 giờ trong khu vực', 'TRANG THIẾT BỊ DỤNG CỤ')
  ])
]

const sections = ref(defaultSections())

sections.value.forEach(section => {
  applyReportTemplate('VeSinh', section.items, `${section.id}. ${section.title}`)
})

const activeSectionIndex = ref(0)
const activeSection = computed(() => sections.value[activeSectionIndex.value])
const activeRows = computed(() => {
  const rows = []
  let currentGroup = ''

  activeSection.value.items.forEach(row => {
    if (row.nhomPhu && row.nhomPhu !== currentGroup) {
      rows.push({
        type: 'group',
        key: `group-${row.nhomPhu}`,
        title: row.nhomPhu
      })
      currentGroup = row.nhomPhu
    }

    rows.push({
      type: 'item',
      key: `item-${row.mucSo}`,
      row
    })
  })

  return rows
})
const totalItems = computed(() => sections.value.reduce((sum, section) => sum + section.items.length, 0))
const completedCount = computed(() => {
  return sections.value.reduce((sum, sec) => sum + sec.items.filter(item => item.dat !== null).length, 0)
})
const totalCount = computed(() => {
  return sections.value.reduce((sum, sec) => sum + sec.items.length, 0)
})
const progressPercent = computed(() => Math.round((completedCount.value / totalCount.value) * 100))

const scrollToFirstUnchecked = async () => {
  let foundSectionIndex = -1
  let foundItem = null

  for (let i = 0; i < sections.value.length; i++) {
    const item = sections.value[i].items.find(it => it.dat === null)
    if (item) {
      foundSectionIndex = i
      foundItem = item
      break
    }
  }

  if (foundItem !== null && foundSectionIndex !== -1) {
    if (activeSectionIndex.value !== foundSectionIndex) {
      activeSectionIndex.value = foundSectionIndex
    }

    await nextTick()

    const el = document.getElementById(`item-row-${foundItem.mucSo}`)
    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'center' })
      el.classList.add('flash-highlight')
      setTimeout(() => el.classList.remove('flash-highlight'), 1800)
    }
  }
}

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
  if (draft.form) {
    const hasTypedData = draft.form.thanhPhans?.some(tp => tp.hoTen?.trim() !== '')
    if (!hasTypedData) {
      draft.form.thanhPhans = [{ stt: 1, hoTen: '', chucVu: '' }]
    }
    form.value = draft.form
  }
  if (Array.isArray(draft.sections) && draft.sections.length >= sections.value.length) sections.value = draft.sections
  if (Number.isInteger(draft.activeSectionIndex)) activeSectionIndex.value = Math.min(draft.activeSectionIndex, sections.value.length - 1)
}

const cancelForm = () => {
  clearFormDraft(DRAFT_KEY)
  router.push('/employee')
}

const submitForm = async () => {
  let hasUnchecked = false
  let uncheckedNum = -1
  for (const sec of sections.value) {
    const item = sec.items.find(it => it.dat === null)
    if (item) {
      hasUnchecked = true
      uncheckedNum = item.mucSo
      break
    }
  }

  if (hasUnchecked) {
    showToast(`Vui lòng hoàn thành tiêu chí số ${uncheckedNum}!`)
    await scrollToFirstUnchecked()
    return
  }

  isSubmitting.value = true

  const chiTiets = sections.value.flatMap(section => {
    return section.items.map(row => ({
      mucSo: row.mucSo,
      phanNhom: `${section.id}. ${section.title}${row.nhomPhu ? ` - ${row.nhomPhu}` : ''}`,
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
      <p class="subtitle">Tại bộ phận chế biến & cung cấp suất ăn</p>
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
                <th>TT</th>
                <th>Nội dung kiểm tra</th>
                <th>Đạt</th>
                <th>Không đạt</th>
                <th>Ghi chú</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in activeSection.items" :key="row.mucSo" :id="'item-row-' + row.mucSo">
                <td class="text-center">{{ row.mucSo }}</td>
                <td>
                  <div class="criteria-text">{{ row.noiDung }}</div>
                </td>
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
        <h3>Góp ý, nhắc nhở của Khoa Dinh dưỡng</h3>
        <textarea v-model="form.gopYKhoaDinhDuong" rows="4" class="glass-input" @focus="scrollFocusedFieldIntoView"></textarea>
      </div>

      <div class="glass-card section-card">
        <h3>Ý kiến của BPCB & CCSA</h3>
        <textarea v-model="form.yKienBPCB" rows="4" class="glass-input" @focus="scrollFocusedFieldIntoView"></textarea>
      </div>

      <div class="form-actions">
        <button type="button" class="btn-secondary" @click="cancelForm">Hủy</button>
        <button type="submit" class="btn-primary" :disabled="isSubmitting">
          <span v-if="!isSubmitting"><ion-icon name="send-outline"></ion-icon> Gửi biên bản lên admin</span>
          <span v-else class="spinner"></span>
        </button>
      </div>
    </form>

    <!-- Sticky Progress Bar -->
    <div class="sticky-progress-bar">
      <div class="progress-info">
        <span>Tiến độ: <strong>{{ completedCount }}/{{ totalCount }}</strong> tiêu chí ({{ progressPercent }}%)</span>
        <button v-if="completedCount < totalCount" type="button" class="btn-goto-missing" @click="scrollToFirstUnchecked">
          Tìm mục chưa tích <ion-icon name="arrow-down-outline"></ion-icon>
        </button>
        <span v-else class="progress-success"><ion-icon name="checkmark-circle-outline"></ion-icon> Đã hoàn thành</span>
      </div>
      <div class="progress-track">
        <div class="progress-fill" :style="{ width: progressPercent + '%' }"></div>
      </div>
    </div>
  </div>
</template>

<style scoped>
<style scoped>
.form-container { display: flex; flex-direction: column; gap: 20px; max-width: 1200px; margin: 0 auto; padding-bottom: 120px !important; }
.form-container form { display: flex; flex-direction: column; gap: 20px; }

/* Sticky Progress Bar */
.sticky-progress-bar {
  position: sticky;
  bottom: 0;
  background: rgba(255, 255, 255, 0.96);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1px solid #cbd5e1;
  border-bottom: none;
  padding: 14px 24px;
  box-shadow: 0 -8px 30px rgba(15, 23, 42, 0.08);
  z-index: 99;
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-top: 30px;
  border-top-left-radius: 16px;
  border-top-right-radius: 16px;
}

.progress-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.95rem;
  color: #334155;
}

.btn-goto-missing {
  background: #eff6ff;
  color: #0284c7;
  border: 1px solid #bae6fd;
  padding: 6px 14px;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 700;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  transition: all 0.2s ease;
}

.btn-goto-missing:hover {
  background: #e0f2fe;
  border-color: #7dd3fc;
}

.progress-success {
  color: #16a34a;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 0.9rem;
}

.progress-track {
  width: 100%;
  height: 8px;
  background: #e2e8f0;
  border-radius: 999px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #38bdf8, #10b981);
  transition: width 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  border-radius: 999px;
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
.glass-card { background: #ffffff; border: 1px solid #e2e8f0; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05), 0 2px 4px -1px rgba(0,0,0,0.03); border-radius: 12px; padding: 24px; color: #334155; }
.header-card { text-align: center; background: linear-gradient(135deg, #e0f2fe, #dcfce7); border-bottom: 3px solid #0ea5e9; }
.header-card h2 { margin: 0 0 10px; font-size: 1.8rem; color: #0f172a; }
.subtitle { color: #475569; font-weight: 600; letter-spacing: 1px; text-transform: uppercase; }
.section-card h3 { margin: 0 0 20px; padding-bottom: 10px; border-bottom: 1px solid #e2e8f0; color: #0f172a; }
.content-header { display: flex; justify-content: space-between; align-items: center; gap: 16px; }
.content-header p { margin: 0; color: #64748b; }
.pager-counter { flex-shrink: 0; color: #0369a1; font-weight: 700; background: #eff6ff; border: 1px solid #bfdbfe; border-radius: 8px; padding: 8px 12px; }
.thanh-phan-list { margin-top: 24px; padding-top: 20px; border-top: 1px solid #e2e8f0; }
.section-topline { display: flex; justify-content: space-between; align-items: center; gap: 16px; margin-bottom: 14px; }
.form-row { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; min-width: 0; }
.form-group { display: flex; flex-direction: column; gap: 8px; margin-bottom: 16px; min-width: 0; }
.glass-input, .glass-input-sm { width: 100%; max-width: 100%; box-sizing: border-box; background: #f8fafc; border: 1px solid #cbd5e1; border-radius: 8px; color: #1e293b; padding: 12px 16px; font-family: inherit; font-size: 0.95rem; }
.glass-input-sm { padding: 8px 12px; font-size: 0.85rem; width: 100%; }
.note-input { display: block; min-height: 44px; line-height: 1.45; resize: vertical; white-space: pre-wrap; overflow-wrap: anywhere; }
.thanh-phan-item { display: flex; gap: 12px; margin-bottom: 12px; align-items: center; padding: 12px; border-radius: 10px; background: #f8fafc; border: 1px solid #e2e8f0; }
.stt-badge { width: 36px; height: 36px; display: flex; align-items: center; justify-content: center; background: #eff6ff; color: #0369a1; border-radius: 50%; font-weight: 700; flex-shrink: 0; }
.flex-1 { flex: 1; }
.btn-icon { background: transparent; border: none; font-size: 1.5rem; cursor: pointer; display: flex; }
.text-red { color: #ef4444; }
.btn-outline { background: transparent; border: 1px solid #94a3b8; color: #475569; padding: 6px 12px; border-radius: 6px; cursor: pointer; font-size: 0.85rem; }
.section-tabs { display: grid; grid-template-columns: repeat(auto-fit, minmax(190px, 1fr)); gap: 10px; margin: 18px 0; }
.section-tab { display: flex; align-items: center; gap: 10px; padding: 10px 12px; border: 1px solid #cbd5e1; border-radius: 8px; background: #f8fafc; cursor: pointer; text-align: left; min-height: 64px; }
.section-tab span { display: inline-flex; align-items: center; justify-content: center; width: 34px; height: 34px; border-radius: 50%; background: #e0f2fe; color: #0369a1; font-weight: 800; flex-shrink: 0; }
.section-tab strong { font-size: 0.78rem; line-height: 1.25; color: #334155; }
.section-tab.active { border-color: #0ea5e9; background: #eff6ff; }
.table-responsive { overflow-x: auto; }
.glass-table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
.glass-table th, .glass-table td { padding: 12px; border: 1px solid #e2e8f0; vertical-align: middle; }
.glass-table th { background: #f8fafc; text-align: left; font-weight: 600; color: #475569; }
.glass-table th:nth-child(1), .glass-table td:nth-child(1) { width: 58px; }
.glass-table th:nth-child(2), .glass-table td:nth-child(2) { min-width: 460px; }
.glass-table th:nth-child(3), .glass-table th:nth-child(4) { width: 100px; text-align: center; }
.glass-table th:nth-child(5), .glass-table td:nth-child(5) { min-width: 170px; }
.subgroup-label { display: block; margin-bottom: 5px; color: #0f172a; font-size: 0.82rem; text-transform: uppercase; }
.criteria-text { line-height: 1.45; white-space: pre-line; }
.text-center { text-align: center !important; }
.section-pager, .form-actions { display: flex; justify-content: flex-end; gap: 15px; margin-top: 16px; }
.btn-primary, .btn-secondary { padding: 12px 24px; border-radius: 8px; font-size: 1rem; font-weight: 600; cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 8px; border: none; }
.btn-primary { background: #0ea5e9; color: white; }
.btn-primary:disabled, .btn-secondary:disabled { opacity: 0.55; cursor: not-allowed; }
.btn-secondary { background: #f1f5f9; color: #475569; border: 1px solid #cbd5e1; }
.spinner { width: 20px; height: 20px; border: 3px solid rgba(255,255,255,0.3); border-top-color: #fff; border-radius: 50%; animation: spin 0.8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 768px) {
  .form-container { padding-bottom: max(320px, env(safe-area-inset-bottom)); }
  .form-row { grid-template-columns: minmax(0, 1fr); width: 100%; max-width: 100%; }
  .glass-card { overflow: hidden; padding: 18px; }
  input[type='date'].glass-input { width: 100%; max-width: 100%; min-width: 0; -webkit-appearance: none; appearance: none; }
  .thanh-phan-item { flex-direction: column; align-items: stretch; gap: 8px; }
  .content-header, .section-pager, .form-actions { flex-direction: column; align-items: stretch; }
  .table-responsive { overflow-x: auto; -webkit-overflow-scrolling: touch; padding-bottom: 8px; }
  .glass-table { min-width: 850px; }
  .note-input { min-width: 180px; min-height: 72px; font-size: 0.95rem; }
  .glass-input:focus, .glass-input-sm:focus { border-color: #38bdf8; box-shadow: 0 0 0 3px rgba(14, 165, 233, 0.18); outline: none; }
}
</style>
