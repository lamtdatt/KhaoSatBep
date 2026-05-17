<script setup>
import { nextTick, onMounted, onUnmounted, ref } from 'vue'
import { clearSignatureProfile, getSignatureProfile, saveSignatureProfile } from '@/utils/signatureStore'

const props = defineProps({
  storageRole: {
    type: String,
    default: 'employee'
  },
  defaultName: {
    type: String,
    default: 'Nhân viên khảo sát'
  },
  defaultRole: {
    type: String,
    default: 'Khối kiểm tra bếp'
  },
  successMessage: {
    type: String,
    default: 'Đã lưu chữ ký điện tử thành công!'
  },
  noteText: {
    type: String,
    default: 'Chữ ký này sẽ là chữ ký của nhân viên khảo sát. Ô chữ ký duyệt của admin sẽ được ghép sau ở bước xuất PDF.'
  }
})

const canvasRef = ref(null)
const signatureName = ref(props.defaultName)
const signatureRole = ref(props.defaultRole)
const savedSignature = ref(null)
const toast = ref({
  visible: false,
  message: ''
})

let drawing = false
let ctx = null
let toastTimer = null

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

const loadSignature = () => {
  const profile = getSignatureProfile(props.storageRole)
  savedSignature.value = profile

  if (profile) {
    signatureName.value = profile.name || props.defaultName
    signatureRole.value = profile.role || props.defaultRole
  }
}

const setupCanvas = () => {
  const canvas = canvasRef.value
  if (!canvas) {
    return
  }

  const ratio = window.devicePixelRatio || 1
  const width = canvas.offsetWidth || 760
  const height = canvas.offsetHeight || 320

  canvas.width = width * ratio
  canvas.height = height * ratio

  ctx = canvas.getContext('2d')
  ctx.setTransform(1, 0, 0, 1, 0, 0)
  ctx.scale(ratio, ratio)
  ctx.lineCap = 'round'
  ctx.lineJoin = 'round'
  ctx.lineWidth = 2.6
  ctx.strokeStyle = '#0f172a'
  ctx.fillStyle = '#ffffff'
  ctx.fillRect(0, 0, width, height)

  if (savedSignature.value?.imageData) {
    const image = new Image()
    image.onload = () => {
      ctx.fillStyle = '#ffffff'
      ctx.fillRect(0, 0, width, height)
      ctx.drawImage(image, 0, 0, width, height)
    }
    image.src = savedSignature.value.imageData
  }
}

const getCanvasPoint = event => {
  const canvas = canvasRef.value
  const rect = canvas.getBoundingClientRect()
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
  const canvas = canvasRef.value
  if (!canvas || !ctx) {
    return
  }

  ctx.clearRect(0, 0, canvas.width, canvas.height)
  const width = canvas.offsetWidth || 760
  const height = canvas.offsetHeight || 320
  ctx.fillStyle = '#ffffff'
  ctx.fillRect(0, 0, width, height)
}

const saveSignature = () => {
  const canvas = canvasRef.value
  if (!canvas) {
    return
  }

  const profile = {
    name: signatureName.value.trim() || props.defaultName,
    role: signatureRole.value.trim() || props.defaultRole,
    imageData: canvas.toDataURL('image/png'),
    updatedAt: new Date().toISOString()
  }

  saveSignatureProfile(profile, props.storageRole)
  savedSignature.value = profile
  showToast(props.successMessage)
}

const removeSignature = async () => {
  clearSignatureProfile(props.storageRole)
  savedSignature.value = null
  await nextTick()
  clearCanvas()
}

onMounted(async () => {
  loadSignature()
  await nextTick()
  setupCanvas()
})

onUnmounted(() => {
  window.clearTimeout(toastTimer)
})
</script>

<template>
  <div class="signature-page">
    <transition name="toast">
      <div v-if="toast.visible" class="success-toast" role="status" aria-live="polite">
        <ion-icon name="checkmark-circle-outline"></ion-icon>
        <span>{{ toast.message }}</span>
      </div>
    </transition>

    <section class="panel signature-workspace">
      <div class="panel-head">
        <div>
          <span class="section-tag">Canvas chữ ký</span>
          <h2>Chữ ký điện tử</h2>
          <p>Viết chữ ký trực tiếp ở khung bên dưới, lưu lại để dùng chung cho các biên bản và xuất PDF sau này.</p>
        </div>
        <div class="panel-badge">
          <ion-icon name="document-lock-outline"></ion-icon>
          <span>Lưu cục bộ trên hệ thống hiện tại</span>
        </div>
      </div>

      <div class="field-grid">
        <label class="field">
          <span>Người ký</span>
          <input v-model="signatureName" type="text" placeholder="Nhập họ tên" />
        </label>
        <label class="field">
          <span>Vai trò</span>
          <input v-model="signatureRole" type="text" placeholder="Nhập chức danh" />
        </label>
      </div>

      <div class="canvas-card">
        <canvas
          ref="canvasRef"
          class="signature-canvas"
          @mousedown="startDrawing"
          @mousemove="draw"
          @mouseup="stopDrawing"
          @mouseleave="stopDrawing"
          @touchstart="startDrawing"
          @touchmove="draw"
          @touchend="stopDrawing"
        ></canvas>
      </div>

      <div class="action-row">
        <button type="button" class="action-btn secondary" @click="clearCanvas">
          <ion-icon name="refresh-outline"></ion-icon>
          <span>Xóa nét ký</span>
        </button>
        <button type="button" class="action-btn secondary danger" @click="removeSignature">
          <ion-icon name="trash-outline"></ion-icon>
          <span>Xóa chữ ký đã lưu</span>
        </button>
        <button type="button" class="action-btn primary" @click="saveSignature">
          <ion-icon name="save-outline"></ion-icon>
          <span>Lưu chữ ký</span>
        </button>
      </div>
    </section>

    <section class="panel preview-panel">
      <div class="panel-head compact">
        <div>
          <span class="section-tag">Xem trước</span>
          <h3>Chữ ký hiện tại</h3>
        </div>
      </div>

      <div v-if="savedSignature?.imageData" class="signature-preview">
        <img :src="savedSignature.imageData" alt="Chữ ký đã lưu" />
        <strong>{{ savedSignature.name }}</strong>
        <span>{{ savedSignature.role }}</span>
      </div>

      <div v-else class="empty-state">
        <ion-icon name="brush-outline"></ion-icon>
        <p>Chưa có chữ ký nào được lưu.</p>
      </div>

      <div class="note-box">
        <strong>Ghi chú</strong>
        <p>{{ noteText }}</p>
      </div>
    </section>
  </div>
</template>

<style scoped>
.signature-page {
  position: relative;
  display: grid;
  grid-template-columns: minmax(0, 1.45fr) minmax(280px, 0.75fr);
  gap: 22px;
}

.success-toast {
  position: fixed;
  top: 24px;
  right: 24px;
  z-index: 50;
  display: inline-flex;
  align-items: center;
  gap: 10px;
  max-width: min(360px, calc(100vw - 32px));
  padding: 14px 16px;
  border: 1px solid #bbf7d0;
  border-radius: 14px;
  background: #f0fdf4;
  color: #166534;
  box-shadow: 0 16px 34px rgba(15, 23, 42, 0.16);
  font-weight: 800;
}

.success-toast ion-icon {
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

.panel {
  background: rgba(255, 255, 255, 0.96);
  border: 1px solid rgba(148, 163, 184, 0.18);
  border-radius: 24px;
  box-shadow: 0 18px 36px rgba(15, 23, 42, 0.08);
  padding: 24px;
}

.panel-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 18px;
}

.panel-head.compact {
  margin-bottom: 18px;
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

.panel-head h2,
.panel-head h3 {
  margin: 12px 0 0;
  color: #0f172a;
}

.panel-head p {
  margin: 10px 0 0;
  max-width: 720px;
  color: #64748b;
  line-height: 1.7;
}

.panel-badge {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 10px 12px;
  border-radius: 14px;
  background: #f8fbff;
  color: #0369a1;
  font-weight: 700;
}

.field-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;
  margin-top: 22px;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.field span {
  color: #475569;
  font-size: 0.9rem;
  font-weight: 700;
}

.field input {
  width: 100%;
  min-height: 46px;
  border: 1px solid #cbd5e1;
  border-radius: 12px;
  padding: 10px 14px;
  font: inherit;
  color: #0f172a;
  background: #f8fafc;
}

.canvas-card {
  margin-top: 18px;
  padding: 12px;
  border: 1px dashed #93c5fd;
  border-radius: 20px;
  background: linear-gradient(180deg, #ffffff, #f8fbff);
}

.signature-canvas {
  display: block;
  width: 100%;
  height: 320px;
  border-radius: 14px;
  background: #ffffff;
  touch-action: none;
  cursor: crosshair;
}

.action-row {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 18px;
}

.action-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  min-height: 44px;
  padding: 10px 16px;
  border-radius: 12px;
  border: 1px solid transparent;
  font-weight: 700;
  cursor: pointer;
}

.action-btn.primary {
  background: linear-gradient(135deg, #0ea5e9, #38bdf8);
  color: #fff;
}

.action-btn.secondary {
  background: #f8fafc;
  color: #475569;
  border-color: #cbd5e1;
}

.action-btn.danger {
  color: #dc2626;
  border-color: #fecaca;
  background: #fff1f2;
}

.signature-preview {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  padding: 18px;
  border: 1px solid #e2e8f0;
  border-radius: 20px;
  background: #f8fbff;
}

.signature-preview img {
  width: 100%;
  max-width: 260px;
  height: 120px;
  object-fit: contain;
  background: #fff;
  border-radius: 12px;
}

.signature-preview strong {
  color: #0f172a;
  text-align: center;
}

.signature-preview span {
  color: #64748b;
  text-align: center;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  min-height: 220px;
  border-radius: 20px;
  background: #f8fbff;
  border: 1px dashed #cbd5e1;
  color: #64748b;
}

.empty-state ion-icon {
  font-size: 2rem;
  color: #0284c7;
}

.note-box {
  margin-top: 18px;
  padding: 16px;
  border-radius: 18px;
  background: linear-gradient(135deg, #fff7ed, #ffedd5);
  color: #9a3412;
}

.note-box strong {
  display: block;
  margin-bottom: 8px;
}

.note-box p {
  line-height: 1.7;
}

@media (max-width: 1080px) {
  .signature-page {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 720px) {
  .success-toast {
    top: 14px;
    right: 14px;
    left: 14px;
    justify-content: center;
  }

  .field-grid {
    grid-template-columns: 1fr;
  }

  .panel-head {
    flex-direction: column;
  }

  .action-row {
    justify-content: stretch;
  }

  .action-btn {
    width: 100%;
  }
}
</style>
