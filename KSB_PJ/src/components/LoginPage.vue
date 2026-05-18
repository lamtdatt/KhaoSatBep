<template>
  <section class="login-section" :class="{ 'is-ready': isReady }" :style="{ backgroundImage: `url(${bannerHM})` }">
    <div class="form-box" :class="{ 'is-leaving': isLeaving }">
      <div class="form-value">
        <form @submit.prevent="handleLogin">
          <div class="login-header">
            <img :src="logo" alt="Logo" class="login-logo" />
            <h2>Login</h2>
          </div>

          <div class="inputbox">
            <ion-icon name="mail-outline"></ion-icon>
            <span class="floating-label">Email</span>
            <input
              id="email-input"
              v-model="email"
              type="email"
              placeholder="Email"
              required
            />
          </div>

          <div class="inputbox">
            <button
              type="button"
              class="password-toggle"
              :aria-label="showPassword ? 'An mat khau' : 'Hien thi mat khau'"
              @click="showPassword = !showPassword"
            >
              <ion-icon :name="showPassword ? 'eye-off-outline' : 'eye-outline'"></ion-icon>
            </button>
            <span class="floating-label">Password</span>
            <input
              id="password-input"
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              placeholder="Password"
              required
            />
          </div>

          <div class="forget">
            <label>
              <input id="remember-checkbox" type="checkbox" v-model="remember" />
              Remember Me
            </label>
          </div>

          <button id="login-btn" type="submit" :disabled="isLoading">
            <span v-if="!isLoading">Log in</span>
            <span v-else class="loading-content">
              <span class="spinner"></span>
              <span>{{ loginStatusMessage }}</span>
            </span>
          </button>

          <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>
        </form>
      </div>
    </div>
  </section>
</template>

<script setup>
import bannerHM from '@/assets/bannerHM.jpg'
import logo from '@/assets/logo.png'
import { apiRequest, warmUpApi } from '@/utils/apiClient'
import { setAuthSession } from '@/utils/authStore'
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'

const REMEMBER_EMAIL_KEY = 'ksb_remembered_email'

const email = ref('')
const password = ref('')
const remember = ref(false)
const showPassword = ref(false)
const isLoading = ref(false)
const isReady = ref(false)
const isLeaving = ref(false)
const errorMessage = ref('')
const loginStatusMessage = ref('Đang đăng nhập...')
const router = useRouter()

onMounted(() => {
  warmUpApi()

  const rememberedEmail = window.localStorage.getItem(REMEMBER_EMAIL_KEY)
  if (rememberedEmail) {
    email.value = rememberedEmail
    remember.value = true
  }

  window.requestAnimationFrame(() => {
    isReady.value = true
  })
})

const handleLogin = async () => {
  if (!email.value || !password.value) {
    return
  }

  isLoading.value = true
  errorMessage.value = ''
  loginStatusMessage.value = 'Đang kết nối máy chủ...'
  const slowLoginTimer = window.setTimeout(() => {
    loginStatusMessage.value = 'Máy chủ phản hồi hơi chậm, vui lòng chờ...'
  }, 5000)

  try {
    const session = await apiRequest('/Auth/dang-nhap', {
      method: 'POST',
      timeoutMs: 15000,
      body: JSON.stringify({
        email: email.value,
        matKhau: password.value
      })
    })

    setAuthSession({
      token: session.token,
      hoTen: session.hoTen,
      email: session.email,
      vaiTro: session.vaiTro
    })

    if (remember.value) {
      window.localStorage.setItem(REMEMBER_EMAIL_KEY, email.value)
    } else {
      window.localStorage.removeItem(REMEMBER_EMAIL_KEY)
    }

    isLeaving.value = true
    await router.push(session.vaiTro === 'Admin' ? '/admin' : '/employee')
  } catch (error) {
    errorMessage.value = error.message || 'Dang nhap that bai'
  } finally {
    window.clearTimeout(slowLoginTimer)
    isLoading.value = false
  }
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600;700&display=swap');

.login-section {
  position: relative;
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100dvh;
  width: 100%;
  padding: 24px;
  background-color: transparent;
  background-repeat: no-repeat;
  background-position: center 42%;
  background-size: 100% auto;
  font-family: 'Poppins', sans-serif;
  isolation: isolate;
  opacity: 0;
  transform: translateY(-24px);
  transition:
    opacity 0.72s ease,
    transform 0.82s cubic-bezier(0.16, 1, 0.3, 1),
    background-position 1.15s cubic-bezier(0.16, 1, 0.3, 1),
    background-size 1.15s cubic-bezier(0.16, 1, 0.3, 1);
}

.login-section::before {
  content: '';
  display: none;
  position: fixed;
  inset: 0 0 auto;
  height: 18svh;
  pointer-events: none;
  background: linear-gradient(180deg, #e6eef5 0%, rgba(214, 234, 248, 0.84) 42%, rgba(214, 234, 248, 0) 100%);
}

.login-section.is-ready {
  opacity: 1;
  transform: translateY(0);
  background-position: center 42%;
  background-size: 100% auto;
}

.form-box {
  position: relative;
  z-index: 2;
  width: min(400px, 100%);
  min-height: 450px;
  background: rgba(0, 0, 0, 0.05);
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 20px;
  backdrop-filter: blur(5px);
  -webkit-backdrop-filter: blur(5px);
  display: flex;
  justify-content: center;
  align-items: center;
  opacity: 0;
  transform: translateY(42px) scale(0.97);
  transition:
    opacity 0.78s ease 0.18s,
    transform 0.86s cubic-bezier(0.16, 1, 0.3, 1) 0.18s,
    box-shadow 0.3s ease;
}

.login-section.is-ready .form-box {
  opacity: 1;
  transform: translateY(0) scale(1);
}

.form-box.is-leaving {
  opacity: 0;
  transform: translateY(-18px) scale(0.985);
  transition:
    opacity 0.3s ease,
    transform 0.36s cubic-bezier(0.4, 0, 0.2, 1);
}

.form-value {
  width: 100%;
  padding: 0 28px;
}

.login-header {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  margin-bottom: 10px;
}

.login-logo {
  width: 45px;
  height: 45px;
  object-fit: contain;
  filter: drop-shadow(0 2px 6px rgba(0,0,0,0.4));
}

h2 {
  font-size: 2em;
  color: #fff;
  margin: 0;
  text-shadow: 0 2px 10px rgba(0,0,0,0.6), 0 0 20px rgba(0,0,0,0.3);
}

.inputbox {
  position: relative;
  display: flex;
  align-items: center;
  margin: 26px 0;
  width: 100%;
  min-height: 52px;
  border-bottom: 2px solid #fff;
}

.inputbox input::placeholder {
  color: transparent;
}

.inputbox input {
  display: block;
  box-sizing: border-box;
  width: 100%;
  height: 44px;
  line-height: 1.2;
  background: transparent;
  border: none;
  outline: none;
  font-size: 1em;
  font-family: inherit;
  font-weight: 400;
  padding: 14px 35px 4px 5px;
  margin: 0;
  color: #fff;
  text-shadow: 0 1px 7px rgba(0, 0, 0, 0.72);
  -webkit-appearance: none;
  appearance: none;
  transform: translateY(0);
  transition: color 0.2s ease;
}

.floating-label {
  position: absolute;
  left: 5px;
  top: 50%;
  z-index: 1;
  color: rgba(255, 255, 255, 0.9);
  font-size: 1em;
  font-weight: 500;
  line-height: 1;
  pointer-events: none;
  transform: translateY(-50%);
  text-shadow: 0 1px 7px rgba(0, 0, 0, 0.62);
  transition:
    top 0.18s ease,
    transform 0.18s ease,
    font-size 0.18s ease,
    color 0.18s ease;
}

.inputbox:focus-within .floating-label,
.inputbox:has(input:not(:placeholder-shown)) .floating-label {
  top: 4px;
  font-size: 0.74rem;
  color: rgba(255, 255, 255, 0.78);
  transform: translateY(0);
}

.inputbox input:-webkit-autofill,
.inputbox input:-webkit-autofill:hover,
.inputbox input:-webkit-autofill:focus {
  -webkit-text-fill-color: #fff;
  caret-color: #fff;
  box-shadow: 0 0 0 1000px transparent inset;
  transition: background-color 9999s ease-in-out 0s;
}

.inputbox > ion-icon {
  position: absolute;
  right: 8px;
  color: #fff;
  font-size: 1.2em;
  top: 50%;
  transform: translateY(-50%);
  filter: drop-shadow(0 1px 4px rgba(0,0,0,0.5));
}

.password-toggle {
  position: absolute;
  right: 0;
  top: 50%;
  z-index: 2;
  width: 34px;
  height: 34px;
  padding: 0;
  border: 0;
  border-radius: 50%;
  background: transparent;
  color: #fff;
  box-shadow: none;
  transform: translateY(-50%);
}

.password-toggle:hover {
  background: rgba(255, 255, 255, 0.14);
  box-shadow: none;
  transform: translateY(-50%);
}

.password-toggle:active {
  transform: translateY(-50%) scale(0.96);
}

.password-toggle ion-icon {
  font-size: 1.2em;
  filter: drop-shadow(0 1px 4px rgba(0,0,0,0.5));
}

.forget {
  margin: -15px 0 15px;
  font-size: 0.9em;
  color: #fff;
  display: flex;
  justify-content: flex-start;
  text-shadow: 0 1px 6px rgba(0,0,0,0.5);
}

.forget label {
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
}

.forget label input {
  margin-right: 3px;
  accent-color: #fff;
  cursor: pointer;
}

button {
  width: 100%;
  height: 40px;
  border-radius: 40px;
  background: #fff;
  color: #06202f;
  border: none;
  outline: none;
  cursor: pointer;
  font-size: 1em;
  font-weight: 600;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(0,0,0,0.2);
}

button span {
  color: #06202f;
  font-weight: 700;
  text-shadow: none;
}

form {
  width: 100%;
}

button:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(0,0,0,0.3);
}

button:active {
  transform: translateY(0);
}

button:disabled {
  opacity: 0.7;
  cursor: not-allowed;
  transform: none;
}

.spinner {
  width: 20px;
  height: 20px;
  border: 3px solid rgba(0,0,0,0.2);
  border-top-color: #333;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

.loading-content {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
}

.error-text {
  margin: 14px 0 0;
  padding: 10px 12px;
  border: 1px solid rgba(220, 38, 38, 0.55);
  border-radius: 12px;
  background: rgba(127, 29, 29, 0.78);
  color: #ffffff;
  text-align: center;
  font-size: 0.92rem;
  font-weight: 800;
  text-shadow: 0 1px 5px rgba(0,0,0,0.55);
  box-shadow: 0 10px 24px rgba(127, 29, 29, 0.24);
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.register {
  font-size: 0.9em;
  color: #fff;
  text-align: center;
  margin: 25px 0 10px;
}

@media (max-width: 480px) {
  .login-section {
    min-height: 100svh;
    align-items: center;
    padding: max(18px, env(safe-area-inset-top)) 18px max(22px, env(safe-area-inset-bottom));
    background-color: #dcecf8;
    background-position: center top;
    background-size: cover;
  }

  .login-section.is-ready {
    background-position: center top;
    background-size: cover;
  }

  .login-section::before {
    display: block;
    z-index: 1;
    height: clamp(96px, 15svh, 142px);
    background:
      linear-gradient(180deg, rgba(232, 239, 245, 0.98) 0%, rgba(220, 237, 249, 0.82) 48%, rgba(220, 237, 249, 0) 100%);
    backdrop-filter: blur(8px) saturate(1.04);
    -webkit-backdrop-filter: blur(8px) saturate(1.04);
    mask-image: linear-gradient(180deg, #000 0%, #000 58%, transparent 100%);
    -webkit-mask-image: linear-gradient(180deg, #000 0%, #000 58%, transparent 100%);
  }

  .form-box {
    width: min(100%, 360px);
    min-height: auto;
    padding: 26px 0 28px;
    border-radius: 18px;
    background: rgba(11, 28, 44, 0.24);
    backdrop-filter: blur(4px) saturate(1.08);
    -webkit-backdrop-filter: blur(4px) saturate(1.08);
    box-shadow: 0 18px 45px rgba(4, 18, 30, 0.18);
  }

  .form-value {
    padding: 0 22px;
  }

  .login-header {
    gap: 2px;
    margin-bottom: 18px;
  }

  .login-logo {
    width: 38px;
    height: 38px;
  }

  h2 {
    font-size: 1.75rem;
  }

  .inputbox {
    margin: 16px 0;
    min-height: 48px;
  }

  .inputbox input {
    height: 42px;
    font-size: 0.98rem;
    font-weight: 600;
    color: #fff;
    text-shadow: 0 1px 8px rgba(0, 0, 0, 0.78);
  }

  .inputbox input:not(:placeholder-shown) {
    padding-left: 10px;
    border-radius: 10px 10px 0 0;
    background: rgba(5, 19, 31, 0.16);
  }

  .inputbox input::placeholder {
    color: transparent;
  }

  .floating-label {
    font-size: 0.98rem;
  }

  .inputbox:focus-within .floating-label,
  .inputbox:has(input:not(:placeholder-shown)) .floating-label {
    top: 3px;
    font-size: 0.72rem;
  }

  .forget {
    margin: 4px 0 18px;
    font-size: 0.95rem;
    gap: 12px;
    flex-wrap: wrap;
  }

  button {
    height: 46px;
    font-size: 1rem;
    color: #06202f;
    background: linear-gradient(180deg, #ffffff 0%, #eef9fc 100%);
    box-shadow: 0 10px 22px rgba(3, 23, 35, 0.28);
  }
}

.register p a {
  color: #fff;
  text-decoration: none;
  font-weight: 600;
}

.register p a:hover {
  text-decoration: underline;
}
</style>
