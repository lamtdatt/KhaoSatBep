<script setup>
</script>

<template>
  <router-view v-slot="{ Component, route }">
    <transition name="route-fade" mode="out-in">
      <component :is="Component" :key="route.fullPath" />
    </transition>
  </router-view>
</template>

<style>
.route-fade-enter-active,
.route-fade-leave-active {
  transition: opacity 0.32s ease, transform 0.32s ease;
}

.route-fade-enter-from {
  opacity: 0;
  transform: translateY(14px);
}

.route-fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>
