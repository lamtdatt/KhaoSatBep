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
  <Teleport to="body">
    <transition name="toast">
      <div v-if="visible" class="app-toast" :class="type" role="status" aria-live="polite">
        <ion-icon :name="type === 'success' ? 'checkmark-circle-outline' : 'information-circle-outline'"></ion-icon>
        <span>{{ message }}</span>
      </div>
    </transition>
  </Teleport>
</template>

<style scoped>
.app-toast {
  position: fixed;
  top: 24px;
  right: 24px;
  z-index: 2147483647;
  display: inline-flex;
  align-items: center;
  gap: 10px;
  max-width: min(420px, calc(100vw - 32px));
  padding: 14px 16px;
  border-radius: 14px;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.24);
  font-weight: 800;
  border-width: 2px;
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
}

.app-toast.success {
  border: 2px solid #22c55e;
  background: rgba(240, 253, 244, 0.97);
  color: #14532d;
}

.app-toast.info {
  border: 2px solid #38bdf8;
  background: rgba(240, 249, 255, 0.98);
  color: #075985;
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
    top: max(14px, env(safe-area-inset-top));
    right: 14px;
    left: 14px;
    justify-content: center;
    text-align: center;
  }
}
</style>
