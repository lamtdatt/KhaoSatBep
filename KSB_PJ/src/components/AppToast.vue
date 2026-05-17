<script setup>
defineProps({
  visible: {
    type: Boolean,
    default: false
  },
  message: {
    type: String,
    default: ''
  },
  type: {
    type: String,
    default: 'success'
  }
})
</script>

<template>
  <transition name="toast">
    <div v-if="visible" class="app-toast" :class="type" role="status" aria-live="polite">
      <ion-icon :name="type === 'success' ? 'checkmark-circle-outline' : 'information-circle-outline'"></ion-icon>
      <span>{{ message }}</span>
    </div>
  </transition>
</template>

<style scoped>
.app-toast {
  position: fixed;
  top: 24px;
  right: 24px;
  z-index: 100;
  display: inline-flex;
  align-items: center;
  gap: 10px;
  max-width: min(420px, calc(100vw - 32px));
  padding: 14px 16px;
  border-radius: 14px;
  box-shadow: 0 16px 34px rgba(15, 23, 42, 0.16);
  font-weight: 800;
}

.app-toast.success {
  border: 1px solid #bbf7d0;
  background: #f0fdf4;
  color: #166534;
}

.app-toast ion-icon {
  flex: 0 0 auto;
  font-size: 1.35rem;
}

.toast-enter-active,
.toast-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

@media (max-width: 720px) {
  .app-toast {
    top: 14px;
    right: 14px;
    left: 14px;
    justify-content: center;
  }
}
</style>
