<script setup>
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { clearAuthSession, getCurrentUser } from '@/utils/authStore'
import { getReports, getUnreadApprovedReports, markApprovedReportsSeen, refreshReports } from '@/utils/reportStore'

const REPORT_TYPE_LABELS = {
  CoSoHaTang: 'Cơ sở hạ tầng',
  HoSo: 'Hồ sơ sổ sách',
  VeSinh: 'Vệ sinh ATTP',
  SuatAnNguoiBenh: 'Suất ăn người bệnh'
}

const router = useRouter()
const route = useRoute()
const contentAreaRef = ref(null)
const isDashboardReady = ref(false)
const isSigningOut = ref(false)
const showMobileBackTop = ref(false)
const approvedNotificationCount = ref(0)
const showNotificationDropdown = ref(false)
const notificationList = ref([])
const notificationDropdownRef = ref(null)
let notificationTimer = null

const user = ref({
  name: 'Nhân viên khảo sát',
  role: 'Khối kiểm tra bếp'
})

const navigationItems = [
  {
    to: '/employee',
    icon: 'bar-chart-outline',
    label: 'Thống kê tổng quan',
    exact: true
  },
  {
    to: '/employee/bb-csht',
    icon: 'business-outline',
    label: 'Cơ sở hạ tầng'
  },
  {
    to: '/employee/bb-hoso',
    icon: 'documents-outline',
    label: 'Hồ sơ sổ sách'
  },
  {
    to: '/employee/bb-vsattp',
    icon: 'shield-checkmark-outline',
    label: 'Vệ sinh ATTP'
  },
  {
    to: '/employee/bb-suatan',
    icon: 'restaurant-outline',
    label: 'Suất ăn người bệnh'
  },
  {
    to: '/employee/chu-ky',
    icon: 'brush-outline',
    label: 'Chữ ký điện tử'
  }
]

const pageTitle = computed(() => {
  const current = navigationItems.find(item => route.path === item.to)
  return current?.label ?? 'Bảng điều khiển'
})

const pageHint = computed(() => {
  if (route.path === '/employee') {
    return 'Theo dõi tiến độ kiểm tra và truy cập nhanh các biểu mẫu đang làm việc.'
  }

  if (route.path === '/employee/chu-ky') {
    return 'Quản lý chữ ký điện tử để dùng lại trên các biên bản và cho bước xuất PDF sau này.'
  }

  return 'Nhập liệu, theo dõi tình trạng và hoàn thiện biểu mẫu ngay trong cùng giao diện.'
})

const isActive = item => {
  if (item.exact) {
    return route.path === item.to
  }

  return route.path.startsWith(item.to)
}

const updateApprovedNotificationCount = () => {
  approvedNotificationCount.value = getUnreadApprovedReports(getReports()).length
}

const refreshApprovedNotifications = async () => {
  try {
    await refreshReports()
  } catch (error) {
    console.error('Khong the tai thong bao nhan vien:', error)
  } finally {
    updateApprovedNotificationCount()
  }
}

const formatNotificationTime = value => {
  if (!value) return ''
  return new Date(value).toLocaleString('vi-VN')
}

const openNotifications = () => {
  const reports = getReports()
  const unread = getUnreadApprovedReports(reports)

  // Build notification list from unread approved reports
  if (unread.length > 0) {
    notificationList.value = unread.map(report => ({
      id: report.id,
      soBienBan: report.soBienBan,
      loaiBienBan: REPORT_TYPE_LABELS[report.loaiBienBan] || report.loaiBienBan,
      time: formatNotificationTime(report.submittedAt || report.updatedAt),
      message: `Admin đã duyệt biên bản ${report.soBienBan} và không có đánh giá gì thêm.`
    }))
  } else {
    // Show already read approved reports as history
    const approved = reports.filter(r => r.status === 'approved').slice(0, 10)
    notificationList.value = approved.map(report => ({
      id: report.id,
      soBienBan: report.soBienBan,
      loaiBienBan: REPORT_TYPE_LABELS[report.loaiBienBan] || report.loaiBienBan,
      time: formatNotificationTime(report.submittedAt || report.updatedAt),
      message: `Admin đã duyệt biên bản ${report.soBienBan} và không có đánh giá gì thêm.`,
      read: true
    }))
  }

  showNotificationDropdown.value = !showNotificationDropdown.value

  // Mark as read after opening
  if (showNotificationDropdown.value && unread.length > 0) {
    markApprovedReportsSeen(reports)
    updateApprovedNotificationCount()
  }
}

const closeNotificationDropdown = () => {
  showNotificationDropdown.value = false
}

const onNotificationClick = (notification) => {
  showNotificationDropdown.value = false
  router.push('/employee')
}

const handleClickOutside = (event) => {
  const dropdown = notificationDropdownRef.value
  const btn = event.target.closest('.notification-btn')
  if (dropdown && !dropdown.contains(event.target) && !btn) {
    showNotificationDropdown.value = false
  }
}

const logout = async () => {
  if (isSigningOut.value) {
    return
  }

  isSigningOut.value = true
  await new Promise(resolve => window.setTimeout(resolve, 320))
  clearAuthSession()
  await router.replace('/login')
}

const updateMobileBackTop = () => {
  showMobileBackTop.value = window.innerWidth <= 720 && window.scrollY > 260
}

const scrollToEmployeeTop = () => {
  window.scrollTo({
    top: 0,
    behavior: 'smooth'
  })
}

onMounted(() => {
  const currentUser = getCurrentUser()
  if (currentUser) {
    user.value = {
      name: currentUser.name,
      role: currentUser.role === 'Admin' ? 'Quan tri he thong' : 'Nhan vien khao sat'
    }
  }

  document.body.classList.add('employee-page-scroll')
  window.requestAnimationFrame(() => {
    isDashboardReady.value = true
  })
  updateMobileBackTop()
  refreshApprovedNotifications()
  notificationTimer = window.setInterval(refreshApprovedNotifications, 30000)
  window.addEventListener('scroll', updateMobileBackTop, { passive: true })
  window.addEventListener('resize', updateMobileBackTop)
  window.addEventListener('ksb-reports-updated', updateApprovedNotificationCount)
  window.addEventListener('ksb-employee-notifications-updated', updateApprovedNotificationCount)
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.body.classList.remove('employee-page-scroll')
  window.clearInterval(notificationTimer)
  window.removeEventListener('scroll', updateMobileBackTop)
  window.removeEventListener('resize', updateMobileBackTop)
  window.removeEventListener('ksb-reports-updated', updateApprovedNotificationCount)
  window.removeEventListener('ksb-employee-notifications-updated', updateApprovedNotificationCount)
  document.removeEventListener('click', handleClickOutside)
})

watch(
  () => route.fullPath,
  async () => {
    await nextTick()
    contentAreaRef.value?.scrollTo({ top: 0, left: 0, behavior: 'auto' })
    window.scrollTo({ top: 0, left: 0, behavior: 'auto' })
  }
)
</script>

<template>
  <div class="dashboard-layout" :class="{ 'is-ready': isDashboardReady, 'is-signing-out': isSigningOut }">
    <button
      v-show="showMobileBackTop"
      type="button"
      class="mobile-back-top"
      aria-label="Quay lại đầu trang"
      @click="scrollToEmployeeTop"
    >
      <ion-icon name="chevron-up-outline"></ion-icon>
    </button>

    <aside class="sidebar">
      <div class="sidebar-header">
        <img src="../assets/logo.png" alt="Logo" class="logo" />
        <div>
          <h3>Khảo Sát Bếp</h3>
          <p>Hệ thống biểu mẫu nội bộ</p>
        </div>
      </div>

      <div class="user-info">
        <div class="avatar">
          <ion-icon name="person-circle-outline"></ion-icon>
        </div>
        <div class="info">
          <p class="name">{{ user.name }}</p>
          <p class="role">{{ user.role }}</p>
        </div>
      </div>

      <nav class="sidebar-nav">
        <div class="nav-section">Điều hướng nhanh</div>

        <router-link
          v-for="item in navigationItems"
          :key="item.to"
          :to="item.to"
          class="nav-item"
          :class="{ active: isActive(item) }"
        >
          <ion-icon :name="item.icon"></ion-icon>
          <span>{{ item.label }}</span>
        </router-link>
      </nav>

      <div class="sidebar-footer">
        <button @click="logout" class="logout-btn">
          <ion-icon name="log-out-outline"></ion-icon>
          <span>Đăng xuất</span>
        </button>
      </div>
    </aside>

    <main class="main-content">
      <header class="topbar">
        <div class="topbar-title">
          <span class="eyebrow">Bảng điều khiển nhân viên</span>
          <h1>{{ pageTitle }}</h1>
          <p>{{ pageHint }}</p>
        </div>

        <div class="topbar-actions">
          <label class="search-bar" for="dashboard-search">
            <ion-icon name="search-outline"></ion-icon>
            <input id="dashboard-search" type="text" placeholder="Tìm kiếm biên bản, khoa phòng..." />
          </label>

          <div class="notification-wrapper">
            <button
              class="icon-btn notification-btn"
              :class="{ 'has-unread': approvedNotificationCount > 0 }"
              type="button"
              :aria-label="`Thông báo: ${approvedNotificationCount} biên bản đã duyệt mới`"
              @click="openNotifications"
            >
              <ion-icon name="notifications-outline"></ion-icon>
              <span v-if="approvedNotificationCount > 0" class="notification-badge">
                {{ approvedNotificationCount > 9 ? '9+' : approvedNotificationCount }}
              </span>
            </button>

            <Teleport to="body">
            <transition name="dropdown">
              <div
                v-if="showNotificationDropdown"
                ref="notificationDropdownRef"
                class="notification-dropdown"
              >
                <div class="notification-dropdown-header">
                  <ion-icon name="notifications" class="noti-header-icon"></ion-icon>
                  <span>Thông báo</span>
                </div>

                <div v-if="notificationList.length" class="notification-dropdown-list">
                  <div
                    v-for="noti in notificationList"
                    :key="noti.id"
                    class="notification-dropdown-item"
                    :class="{ 'is-read': noti.read }"
                    @click="onNotificationClick(noti)"
                  >
                    <div class="noti-icon-wrap">
                      <ion-icon name="checkmark-done-circle" class="noti-icon"></ion-icon>
                    </div>
                    <div class="noti-content">
                      <div class="noti-title">
                        <strong>{{ noti.loaiBienBan }}</strong>
                        <span class="noti-bb">{{ noti.soBienBan }}</span>
                      </div>
                      <p class="noti-message">{{ noti.message }}</p>
                      <span class="noti-time">
                        <ion-icon name="time-outline"></ion-icon>
                        {{ noti.time }}
                      </span>
                    </div>
                  </div>
                </div>

                <div v-else class="notification-dropdown-empty">
                  <ion-icon name="checkmark-circle-outline"></ion-icon>
                  <p>Không có thông báo mới</p>
                </div>
              </div>
            </transition>
            </Teleport>
          </div>
        </div>
      </header>

      <div ref="contentAreaRef" class="content-area">
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </div>
    </main>
  </div>
</template>

<style scoped>
:global(body.employee-page-scroll) {
  overflow: auto;
}

.dashboard-layout {
  display: grid;
  grid-template-columns: 280px minmax(0, 1fr);
  width: 100%;
  height: 100dvh;
  min-height: 100dvh;
  overflow: hidden;
  background:
    radial-gradient(circle at top left, rgba(14, 165, 233, 0.12), transparent 30%),
    linear-gradient(180deg, #f5f9fd 0%, #edf4fb 100%);
  color: #0f172a;
  transition:
    opacity 0.32s ease,
    transform 0.36s cubic-bezier(0.4, 0, 0.2, 1),
    filter 0.36s ease;
}

.dashboard-layout.is-signing-out {
  opacity: 0;
  transform: translateY(14px) scale(0.985);
  filter: blur(5px);
  pointer-events: none;
}

.mobile-back-top {
  display: none;
}

.sidebar {
  display: flex;
  flex-direction: column;
  min-height: 0;
  height: 100dvh;
  background: rgba(255, 255, 255, 0.96);
  border-right: 1px solid rgba(148, 163, 184, 0.24);
  box-shadow: 18px 0 38px rgba(15, 23, 42, 0.06);
  overflow: hidden;
  opacity: 0;
  transform: translateX(-26px);
  transition:
    opacity 0.62s ease,
    transform 0.72s cubic-bezier(0.16, 1, 0.3, 1);
}

.dashboard-layout.is-ready .sidebar {
  opacity: 1;
  transform: translateX(0);
}

.sidebar-header {
  padding: 26px 24px 22px;
  display: flex;
  align-items: center;
  gap: 14px;
  border-bottom: 1px solid #e2e8f0;
}

.logo {
  height: 40px;
  width: 40px;
  object-fit: contain;
}

.sidebar-header h3 {
  margin: 0;
  font-size: 1.35rem;
  font-weight: 800;
  color: #1e293b;
}

.sidebar-header p {
  margin: 4px 0 0;
  color: #64748b;
  font-size: 0.88rem;
}

.user-info {
  margin: 18px 20px 0;
  padding: 18px;
  display: flex;
  align-items: center;
  gap: 14px;
  border: 1px solid #dbeafe;
  border-radius: 18px;
  background: linear-gradient(135deg, #f8fbff 0%, #eef6ff 100%);
}

.avatar {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 54px;
  height: 54px;
  border-radius: 16px;
  background: linear-gradient(135deg, #0ea5e9, #38bdf8);
  color: #fff;
  box-shadow: 0 12px 28px rgba(14, 165, 233, 0.24);
}

.avatar ion-icon {
  font-size: 2rem;
}

.name {
  margin: 0;
  font-size: 1rem;
  font-weight: 700;
  color: #0f172a;
}

.role {
  margin: 4px 0 0;
  font-size: 0.9rem;
  color: #475569;
}

.sidebar-nav {
  flex: 1;
  padding: 22px 0;
  overflow-y: auto;
}

.sidebar-nav::-webkit-scrollbar {
  width: 6px;
}

.sidebar-nav::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 999px;
}

.nav-section {
  padding: 0 24px 10px;
  font-size: 0.78rem;
  font-weight: 800;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.nav-item {
  position: relative;
  display: flex;
  align-items: center;
  gap: 12px;
  margin: 4px 14px;
  padding: 14px 16px;
  border-radius: 14px;
  color: #334155;
  text-decoration: none;
  font-weight: 600;
  transition: transform 0.2s ease, background 0.2s ease, color 0.2s ease, box-shadow 0.2s ease;
}

.nav-item:hover {
  background: #f8fbff;
  color: #0369a1;
  transform: translateX(2px);
}

.nav-item.active {
  color: #0369a1;
  background: linear-gradient(135deg, #e0f2fe, #f0f9ff);
  box-shadow: inset 0 0 0 1px rgba(14, 165, 233, 0.2);
}

.nav-item.active::before {
  content: '';
  position: absolute;
  left: -14px;
  top: 10px;
  bottom: 10px;
  width: 4px;
  border-radius: 999px;
  background: linear-gradient(180deg, #0ea5e9, #0284c7);
}

.nav-item ion-icon {
  font-size: 1.2rem;
}

.sidebar-footer {
  padding: 18px 20px 24px;
  border-top: 1px solid #e2e8f0;
}

.logout-btn {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 14px 16px;
  border: 1px solid #fecaca;
  border-radius: 14px;
  background: linear-gradient(135deg, #fff1f2, #ffe4e6);
  color: #dc2626;
  font-size: 0.95rem;
  font-weight: 700;
  cursor: pointer;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.logout-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 10px 18px rgba(220, 38, 38, 0.12);
}

.main-content {
  min-width: 0;
  min-height: 0;
  height: 100dvh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
  padding: 24px 30px 18px;
  opacity: 0;
  transform: translateY(-18px);
  transition:
    opacity 0.58s ease 0.12s,
    transform 0.68s cubic-bezier(0.16, 1, 0.3, 1) 0.12s;
}

.dashboard-layout.is-ready .topbar {
  opacity: 1;
  transform: translateY(0);
}

.topbar-title {
  min-width: 0;
}

.eyebrow {
  display: inline-block;
  margin-bottom: 8px;
  color: #0284c7;
  font-size: 0.82rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.12em;
}

.topbar-title h1 {
  margin: 0;
  font-size: 1.9rem;
  line-height: 1.15;
  color: #0f172a;
}

.topbar-title p {
  margin: 8px 0 0;
  max-width: 720px;
  color: #475569;
  font-size: 0.98rem;
}

.topbar-actions {
  display: flex;
  align-items: center;
  gap: 14px;
}

.search-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 320px;
  padding: 12px 16px;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.96);
  border: 1px solid rgba(148, 163, 184, 0.24);
  box-shadow: 0 10px 24px rgba(15, 23, 42, 0.05);
}

.search-bar ion-icon {
  color: #64748b;
  font-size: 1.1rem;
}

.search-bar input {
  width: 100%;
  border: none;
  outline: none;
  background: transparent;
  font: inherit;
  color: #0f172a;
}

.search-bar input::placeholder {
  color: #94a3b8;
}

.icon-btn {
  position: relative;
  width: 46px;
  height: 46px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: 1px solid rgba(148, 163, 184, 0.24);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.96);
  color: #334155;
  cursor: pointer;
  box-shadow: 0 10px 24px rgba(15, 23, 42, 0.05);
  transition: transform 0.2s ease, color 0.2s ease, box-shadow 0.2s ease;
}

.icon-btn:hover {
  color: #0284c7;
  transform: translateY(-1px);
  box-shadow: 0 14px 28px rgba(15, 23, 42, 0.08);
}

.icon-btn ion-icon {
  font-size: 1.2rem;
}

.notification-wrapper {
  position: relative;
}

.notification-btn.has-unread {
  color: #0284c7;
  box-shadow: 0 16px 30px rgba(14, 165, 233, 0.16);
}

.notification-badge {
  position: absolute;
  top: -7px;
  right: -7px;
  min-width: 22px;
  height: 22px;
  padding: 0 6px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: 2px solid #ffffff;
  border-radius: 999px;
  background: linear-gradient(135deg, #ef4444, #dc2626);
  color: #ffffff;
  font-size: 0.72rem;
  font-weight: 900;
  line-height: 1;
  box-shadow: 0 8px 18px rgba(220, 38, 38, 0.32);
  animation: notificationPulse 1.35s ease-in-out infinite;
}

@keyframes notificationPulse {
  0%,
  100% {
    transform: scale(1);
  }

  50% {
    transform: scale(1.1);
  }
}

/* Notification Dropdown */
.notification-dropdown {
  position: fixed;
  top: 82px;
  right: 30px;
  z-index: 2147483000;
  width: 400px;
  max-height: 480px;
  display: flex;
  flex-direction: column;
  background: rgba(255, 255, 255, 0.98);
  border: 1px solid rgba(148, 163, 184, 0.2);
  border-radius: 20px;
  box-shadow:
    0 25px 60px rgba(15, 23, 42, 0.15),
    0 8px 20px rgba(15, 23, 42, 0.06);
  backdrop-filter: blur(16px);
  overflow: hidden;
}

.notification-dropdown-header {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 18px 20px;
  border-bottom: 1px solid rgba(148, 163, 184, 0.16);
  background: linear-gradient(135deg, #f0f9ff, #e0f2fe);
}

.noti-header-icon {
  font-size: 1.3rem;
  color: #0284c7;
}

.notification-dropdown-header span {
  font-size: 1.05rem;
  font-weight: 800;
  color: #0f172a;
}

.notification-dropdown-list {
  flex: 1;
  overflow-y: auto;
  padding: 8px;
}

.notification-dropdown-list::-webkit-scrollbar {
  width: 5px;
}

.notification-dropdown-list::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 999px;
}

.notification-dropdown-item {
  display: flex;
  align-items: flex-start;
  gap: 14px;
  padding: 14px 14px;
  border-radius: 14px;
  cursor: pointer;
  transition: background 0.2s ease, transform 0.15s ease;
}

.notification-dropdown-item:hover {
  background: linear-gradient(135deg, #f0f9ff, #f8fbff);
  transform: translateX(2px);
}

.notification-dropdown-item.is-read {
  opacity: 0.6;
}

.noti-icon-wrap {
  width: 42px;
  height: 42px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 14px;
  background: linear-gradient(135deg, #dcfce7, #bbf7d0);
  flex-shrink: 0;
}

.noti-icon {
  font-size: 1.3rem;
  color: #16a34a;
}

.noti-content {
  flex: 1;
  min-width: 0;
}

.noti-title {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.noti-title strong {
  font-size: 0.92rem;
  color: #0f172a;
}

.noti-bb {
  padding: 3px 8px;
  border-radius: 8px;
  background: #eff6ff;
  color: #0369a1;
  font-size: 0.78rem;
  font-weight: 700;
}

.noti-message {
  margin: 6px 0 0;
  color: #475569;
  font-size: 0.88rem;
  line-height: 1.5;
}

.noti-time {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  margin-top: 6px;
  color: #94a3b8;
  font-size: 0.8rem;
}

.noti-time ion-icon {
  font-size: 0.85rem;
}

.notification-dropdown-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 40px 20px;
  color: #64748b;
}

.notification-dropdown-empty ion-icon {
  font-size: 2.4rem;
  color: #10b981;
}

.notification-dropdown-empty p {
  margin: 0;
  font-size: 0.94rem;
  font-weight: 600;
}

/* Dropdown transition */
.dropdown-enter-active {
  transition: opacity 0.25s ease, transform 0.25s cubic-bezier(0.16, 1, 0.3, 1);
}

.dropdown-leave-active {
  transition: opacity 0.18s ease, transform 0.18s ease;
}

.dropdown-enter-from {
  opacity: 0;
  transform: translateY(-10px) scale(0.96);
}

.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-6px) scale(0.98);
}

.content-area {
  flex: 1;
  min-height: 0;
  padding: 6px 30px 30px;
  overflow-y: auto;
  overscroll-behavior: contain;
  opacity: 0;
  transform: translateY(24px);
  transition:
    opacity 0.66s ease 0.24s,
    transform 0.76s cubic-bezier(0.16, 1, 0.3, 1) 0.24s;
}

.dashboard-layout.is-ready .content-area {
  opacity: 1;
  transform: translateY(0);
}

.content-area::-webkit-scrollbar {
  width: 10px;
}

.content-area::-webkit-scrollbar-track {
  background: transparent;
}

.content-area::-webkit-scrollbar-thumb {
  background: rgba(148, 163, 184, 0.5);
  border-radius: 999px;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.24s ease, transform 0.24s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
  transform: translateY(10px);
}

@media (max-width: 1180px) {
  .notification-dropdown {
    width: 340px;
  }

  .dashboard-layout {
    grid-template-columns: 240px minmax(0, 1fr);
  }

  .search-bar {
    min-width: 240px;
  }
}

@media (max-width: 960px) {
  :global(html),
  :global(body),
  :global(#app) {
    height: auto;
    min-height: 100dvh;
  }

  :global(body.employee-page-scroll) {
    overflow-x: hidden;
    overflow-y: auto;
    -webkit-overflow-scrolling: touch;
  }

  .dashboard-layout {
    display: block;
    grid-template-columns: 1fr;
    height: auto;
    min-height: 100dvh;
    overflow: visible;
  }

  .sidebar {
    height: auto;
    min-height: auto;
    overflow: visible;
    border-right: none;
    border-bottom: 1px solid rgba(148, 163, 184, 0.24);
    box-shadow: 0 14px 32px rgba(15, 23, 42, 0.06);
  }

  .main-content {
    height: auto;
    min-height: 100dvh;
    overflow: visible;
  }

  .content-area {
    flex: none;
    min-height: auto;
    overflow: visible;
  }

  .topbar {
    flex-direction: column;
    align-items: stretch;
  }

  .topbar-actions {
    justify-content: space-between;
  }

  .search-bar {
    min-width: 0;
    flex: 1;
  }
}

@media (max-width: 720px) {
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

  .sidebar {
    padding: 0;
  }

  .sidebar-header {
    justify-content: center;
    padding: 18px 16px 14px;
  }

  .sidebar-header h3 {
    font-size: 1.2rem;
  }

  .sidebar-header p {
    font-size: 0.8rem;
  }

  .logo {
    width: 36px;
    height: 36px;
  }

  .user-info {
    margin: 14px 14px 0;
    padding: 13px;
    border-radius: 14px;
  }

  .avatar {
    width: 46px;
    height: 46px;
    border-radius: 14px;
  }

  .name {
    font-size: 0.94rem;
  }

  .role {
    font-size: 0.82rem;
  }

  .sidebar-nav {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 8px;
    padding: 16px 14px;
    overflow: visible;
  }

  .nav-section {
    grid-column: 1 / -1;
    padding: 0;
    font-size: 0.72rem;
  }

  .nav-item {
    min-height: 48px;
    margin: 0;
    padding: 10px 11px;
    border-radius: 12px;
    font-size: 0.88rem;
    line-height: 1.25;
  }

  .nav-item:hover {
    transform: none;
  }

  .nav-item.active::before {
    left: 0;
    top: auto;
    right: 0;
    bottom: 0;
    width: auto;
    height: 3px;
  }

  .sidebar-footer {
    padding: 0 14px 16px;
    border-top: none;
  }

  .logout-btn {
    min-height: 44px;
    padding: 11px;
    border-radius: 12px;
  }

  .topbar {
    gap: 14px;
    padding: 18px 14px 12px;
  }

  .eyebrow {
    font-size: 0.72rem;
  }

  .topbar-title h1 {
    font-size: 1.55rem;
  }

  .topbar-title p {
    font-size: 0.9rem;
    line-height: 1.45;
  }

  .topbar-actions {
    gap: 10px;
  }

  .search-bar {
    min-height: 42px;
    padding: 10px 12px;
    border-radius: 12px;
  }

  .search-bar input {
    font-size: 0.9rem;
  }

  .icon-btn {
    width: 42px;
    height: 42px;
    border-radius: 12px;
    flex: 0 0 auto;
  }

  .notification-dropdown {
    top: max(76px, calc(env(safe-area-inset-top) + 62px));
    right: 14px;
    left: 14px;
    width: auto;
    max-height: min(520px, calc(100dvh - 110px));
  }

  .content-area {
    padding: 4px 14px 28px;
  }
}

@media (max-width: 420px) {
  .sidebar-nav {
    grid-template-columns: 1fr;
  }

  .topbar-actions {
    align-items: stretch;
  }

  .search-bar {
    min-width: 0;
  }

  .topbar-title h1 {
    font-size: 1.38rem;
  }

  .content-area {
    padding: 2px 10px 26px;
  }
}
</style>
