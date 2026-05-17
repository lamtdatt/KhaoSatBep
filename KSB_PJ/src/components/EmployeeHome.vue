<script setup>
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { getReports, refreshReports } from '@/utils/reportStore'

const reports = ref([])

const reportTypes = [
  {
    type: 'CoSoHaTang',
    label: 'Cơ sở hạ tầng',
    color: 'linear-gradient(180deg, #0ea5e9 0%, #38bdf8 100%)'
  },
  {
    type: 'HoSo',
    label: 'Hồ sơ sổ sách',
    color: 'linear-gradient(180deg, #14b8a6 0%, #2dd4bf 100%)'
  },
  {
    type: 'VeSinh',
    label: 'Vệ sinh ATTP',
    color: 'linear-gradient(180deg, #f97316 0%, #fb923c 100%)'
  },
  {
    type: 'SuatAnNguoiBenh',
    label: 'Suất ăn người bệnh',
    color: 'linear-gradient(180deg, #8b5cf6 0%, #a78bfa 100%)'
  }
]

const typeLabels = Object.fromEntries(reportTypes.map(item => [item.type, item.label]))

const loadReports = async () => {
  await refreshReports()
  reports.value = getReports()
}

const isThisMonth = report => {
  const date = new Date(report.submittedAt || report.updatedAt || report.ngayKiemTra)
  const now = new Date()

  return date.getMonth() === now.getMonth() && date.getFullYear() === now.getFullYear()
}

const monthReports = computed(() => reports.value.filter(isThisMonth))
const approvedReports = computed(() => reports.value.filter(report => report.status === 'approved'))
const pendingReports = computed(() => reports.value.filter(report => report.status === 'submitted'))
const reviewedReports = computed(() => reports.value.filter(report => report.status === 'reviewed'))

const overviewStats = computed(() => {
  return reportTypes.map(item => {
    const count = reports.value.filter(report => report.loaiBienBan === item.type).length

    return {
      label: item.label,
      value: count,
      color: item.color,
      note: count ? `${count} biên bản đã gửi` : 'Chưa có dữ liệu'
    }
  })
})

const summaryCards = computed(() => [
  {
    title: 'Tổng biên bản tháng này',
    value: String(monthReports.value.length),
    delta: monthReports.value.length ? 'Đã ghi nhận dữ liệu' : 'Chưa có dữ liệu',
    icon: 'document-text-outline',
    tone: 'blue'
  },
  {
    title: 'Đã duyệt',
    value: String(approvedReports.value.length),
    delta: approvedReports.value.length ? 'Admin đã duyệt' : 'Chưa có dữ liệu',
    icon: 'checkmark-done-circle-outline',
    tone: 'green'
  },
  {
    title: 'Chờ duyệt',
    value: String(pendingReports.value.length),
    delta: pendingReports.value.length ? 'Đang chờ admin xử lý' : 'Chưa có dữ liệu',
    icon: 'time-outline',
    tone: 'amber'
  },
  {
    title: 'Cần chỉnh sửa',
    value: String(reviewedReports.value.length),
    delta: reviewedReports.value.length ? 'Admin đã lưu chỉnh sửa' : 'Chưa có dữ liệu',
    icon: 'alert-circle-outline',
    tone: 'rose'
  }
])

const moduleCards = [
  {
    title: 'Cơ sở hạ tầng',
    route: '/employee/bb-csht',
    icon: 'business-outline',
    description: 'Kiểm tra khu bếp, kho, lối đi, điện nước và các điểm cần khắc phục.',
    status: 'Sẵn sàng nhập liệu'
  },
  {
    title: 'Hồ sơ sổ sách',
    route: '/employee/bb-hoso',
    icon: 'documents-outline',
    description: 'Theo dõi giấy tờ pháp lý, sổ kiểm thực, nhật ký vệ sinh và hồ sơ lưu mẫu.',
    status: 'Sẵn sàng nhập liệu'
  },
  {
    title: 'Vệ sinh ATTP',
    route: '/employee/bb-vsattp',
    icon: 'shield-checkmark-outline',
    description: 'Đánh giá con người, dụng cụ, môi trường bếp và các tiêu chí an toàn thực phẩm.',
    status: 'Sẵn sàng nhập liệu'
  },
  {
    title: 'Suất ăn người bệnh',
    route: '/employee/bb-suatan',
    icon: 'restaurant-outline',
    description: 'Kiểm soát chia suất, vận chuyển, giao nhận và phản hồi của khoa điều trị.',
    status: 'Sẵn sàng nhập liệu'
  }
]

const getStatusLabel = status => {
  if (status === 'approved') {
    return 'Đã duyệt'
  }

  if (status === 'reviewed') {
    return 'Đã chỉnh sửa'
  }

  return 'Chờ duyệt'
}

const getStatusTone = status => {
  if (status === 'approved') {
    return 'green'
  }

  if (status === 'reviewed') {
    return 'rose'
  }

  return 'amber'
}

const formatTime = value => {
  if (!value) {
    return 'Chưa rõ thời gian'
  }

  return new Date(value).toLocaleString('vi-VN')
}

const recentActivities = computed(() => {
  return reports.value.slice(0, 5).map(report => ({
    title: `${typeLabels[report.loaiBienBan] || report.loaiBienBan} - ${report.soBienBan}`,
    time: formatTime(report.submittedAt || report.updatedAt),
    status: getStatusLabel(report.status),
    tone: getStatusTone(report.status)
  }))
})

const maxValue = computed(() => Math.max(...overviewStats.value.map(item => item.value), 1))

onMounted(() => {
  loadReports()
  window.addEventListener('ksb-reports-updated', loadReports)
  window.addEventListener('storage', loadReports)
})

onUnmounted(() => {
  window.removeEventListener('ksb-reports-updated', loadReports)
  window.removeEventListener('storage', loadReports)
})
</script>

<template>
  <div class="dashboard-home">
    <section class="summary-grid">
      <article v-for="card in summaryCards" :key="card.title" class="summary-card" :class="`tone-${card.tone}`">
        <div class="summary-icon">
          <ion-icon :name="card.icon"></ion-icon>
        </div>
        <div class="summary-content">
          <span class="summary-label">{{ card.title }}</span>
          <strong class="summary-value">{{ card.value }}</strong>
          <span class="summary-delta">{{ card.delta }}</span>
        </div>
      </article>
    </section>

    <section class="content-grid">
      <article class="panel chart-panel">
        <div class="panel-head">
          <div>
            <span class="section-tag">Biểu đồ cột</span>
            <h3>Thống kê tổng quan theo module</h3>
          </div>
          <p>Dữ liệu được tổng hợp từ các biên bản nhân viên đã gửi lên admin.</p>
        </div>

        <div class="chart">
          <div v-for="item in overviewStats" :key="item.label" class="chart-item">
            <span class="chart-value">{{ item.value }}</span>
            <div class="chart-bar-wrap">
              <div
                class="chart-bar"
                :class="{ empty: item.value === 0 }"
                :style="{
                  height: `${(item.value / maxValue) * 100}%`,
                  background: item.color
                }"
              ></div>
            </div>
            <strong class="chart-label">{{ item.label }}</strong>
            <span class="chart-note">{{ item.note }}</span>
          </div>
        </div>
      </article>

      <article class="panel activity-panel">
        <div class="panel-head">
          <div>
            <span class="section-tag">Hoạt động gần đây</span>
            <h3>Những mục cần chú ý trong ngày</h3>
          </div>
          <p>Các biên bản mới gửi và trạng thái xử lý sẽ xuất hiện tại đây.</p>
        </div>

        <div v-if="recentActivities.length" class="activity-list">
          <div v-for="activity in recentActivities" :key="activity.title" class="activity-item">
            <div class="activity-copy">
              <strong>{{ activity.title }}</strong>
              <span>{{ activity.time }}</span>
            </div>
            <span class="activity-status" :class="`tone-${activity.tone}`">{{ activity.status }}</span>
          </div>
        </div>

        <div v-else class="empty-state">
          <ion-icon name="file-tray-outline"></ion-icon>
          <p>Chưa có dữ liệu hoạt động để hiển thị.</p>
        </div>
      </article>
    </section>

    <section class="module-section">
      <div class="module-head">
        <div>
          <span class="section-tag">Tất cả module</span>
          <h3>Khu vực làm việc của từng biên bản</h3>
        </div>
        <p>Các module đã sẵn sàng để bạn nhập liệu thật.</p>
      </div>

      <div class="module-grid">
        <router-link
          v-for="module in moduleCards"
          :key="module.route"
          :to="module.route"
          class="module-card"
        >
          <div class="module-icon">
            <ion-icon :name="module.icon"></ion-icon>
          </div>
          <div class="module-copy">
            <div class="module-topline">
              <h4>{{ module.title }}</h4>
              <span>{{ module.status }}</span>
            </div>
            <p>{{ module.description }}</p>
          </div>
          <ion-icon name="arrow-forward-outline" class="module-arrow"></ion-icon>
        </router-link>
      </div>
    </section>
  </div>
</template>

<style scoped>
.dashboard-home {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.panel,
.module-section {
  background: rgba(255, 255, 255, 0.94);
  border: 1px solid rgba(148, 163, 184, 0.18);
  border-radius: 24px;
  box-shadow: 0 20px 45px rgba(15, 23, 42, 0.08);
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 18px;
}

.summary-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 22px;
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.96);
  border: 1px solid rgba(148, 163, 184, 0.16);
  box-shadow: 0 16px 35px rgba(15, 23, 42, 0.06);
}

.summary-icon {
  width: 58px;
  height: 58px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 18px;
  color: #fff;
  font-size: 1.65rem;
  flex-shrink: 0;
}

.tone-blue .summary-icon { background: linear-gradient(135deg, #0ea5e9, #38bdf8); }
.tone-green .summary-icon { background: linear-gradient(135deg, #10b981, #34d399); }
.tone-amber .summary-icon { background: linear-gradient(135deg, #f59e0b, #fbbf24); }
.tone-rose .summary-icon { background: linear-gradient(135deg, #ef4444, #fb7185); }

.summary-content {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.summary-label {
  color: #475569;
  font-size: 0.94rem;
  font-weight: 700;
}

.summary-value {
  color: #0f172a;
  font-size: 2rem;
  line-height: 1;
}

.summary-delta {
  color: #94a3b8;
  font-size: 0.86rem;
}

.content-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.6fr) minmax(320px, 0.9fr);
  gap: 20px;
}

.panel {
  padding: 24px 24px 28px;
}

.panel-head,
.module-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 18px;
}

.panel-head h3,
.module-head h3 {
  margin: 12px 0 0;
  color: #0f172a;
  font-size: 1.35rem;
}

.panel-head p,
.module-head p {
  max-width: 320px;
  color: #64748b;
  line-height: 1.6;
}

.section-tag {
  display: inline-flex;
  align-items: center;
  padding: 6px 10px;
  border-radius: 999px;
  background: #eff6ff;
  color: #0369a1;
  font-size: 0.76rem;
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.chart {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
  align-items: end;
  min-height: 360px;
  padding-top: 20px;
}

.chart-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  min-width: 0;
}

.chart-value {
  color: #0f172a;
  font-size: 1.2rem;
  font-weight: 800;
}

.chart-bar-wrap {
  display: flex;
  align-items: flex-end;
  justify-content: center;
  width: 100%;
  height: 220px;
  padding: 12px;
  border-radius: 18px;
  background: linear-gradient(180deg, rgba(226, 232, 240, 0.28) 0%, rgba(241, 245, 249, 0.78) 100%);
}

.chart-bar {
  width: min(88px, 100%);
  min-height: 0;
  border-radius: 18px 18px 8px 8px;
  box-shadow: 0 14px 30px rgba(15, 23, 42, 0.12);
}

.chart-bar.empty {
  opacity: 0.18;
}

.chart-label {
  text-align: center;
  color: #0f172a;
  font-size: 0.96rem;
  font-weight: 800;
}

.chart-note {
  text-align: center;
  color: #94a3b8;
  font-size: 0.84rem;
  line-height: 1.5;
}

.activity-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
  margin-top: 24px;
}

.activity-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  padding: 18px;
  border-radius: 18px;
  background: linear-gradient(135deg, #f8fbff, #f1f5f9);
  border: 1px solid rgba(148, 163, 184, 0.16);
}

.activity-copy {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.activity-copy strong {
  color: #0f172a;
  font-size: 0.98rem;
}

.activity-copy span {
  color: #64748b;
  font-size: 0.88rem;
}

.activity-status {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 110px;
  padding: 8px 12px;
  border-radius: 999px;
  font-size: 0.84rem;
  font-weight: 800;
}

.activity-status.tone-green {
  background: #dcfce7;
  color: #166534;
}

.activity-status.tone-amber {
  background: #fef3c7;
  color: #92400e;
}

.activity-status.tone-rose {
  background: #ffe4e6;
  color: #be123c;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  min-height: 240px;
  margin-top: 24px;
  border-radius: 18px;
  border: 1px dashed #cbd5e1;
  background: #f8fbff;
  color: #64748b;
}

.empty-state ion-icon {
  font-size: 2rem;
  color: #94a3b8;
}

.module-section {
  padding: 24px;
}

.module-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
  margin-top: 22px;
}

.module-card {
  display: flex;
  align-items: flex-start;
  gap: 16px;
  padding: 20px;
  border-radius: 20px;
  background: linear-gradient(135deg, #ffffff, #f8fbff);
  border: 1px solid rgba(148, 163, 184, 0.18);
  text-decoration: none;
  box-shadow: 0 16px 32px rgba(15, 23, 42, 0.05);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.module-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 22px 34px rgba(15, 23, 42, 0.08);
}

.module-icon {
  width: 54px;
  height: 54px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 18px;
  background: linear-gradient(135deg, #dbeafe, #e0f2fe);
  color: #0369a1;
  font-size: 1.45rem;
  flex-shrink: 0;
}

.module-copy {
  min-width: 0;
  flex: 1;
}

.module-topline {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.module-topline h4 {
  margin: 0;
  color: #0f172a;
  font-size: 1.05rem;
}

.module-topline span {
  padding: 6px 10px;
  border-radius: 999px;
  background: #ecfeff;
  color: #0f766e;
  font-size: 0.74rem;
  font-weight: 800;
  white-space: nowrap;
}

.module-copy p {
  margin: 10px 0 0;
  color: #475569;
  line-height: 1.65;
}

.module-arrow {
  color: #94a3b8;
  font-size: 1.2rem;
  flex-shrink: 0;
}

@media (max-width: 1180px) {
  .summary-grid,
  .chart {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .content-grid,
  .module-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 860px) {
  .panel-head,
  .module-head,
  .activity-item,
  .module-topline {
    flex-direction: column;
  }

  .summary-grid,
  .chart {
    grid-template-columns: 1fr;
  }
}
</style>
