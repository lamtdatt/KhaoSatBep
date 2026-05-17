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
            <span v-else class="spinner"></span>
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
import { apiRequest } from '@/utils/apiClient'
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
const router = useRouter()

onMounted(() => {
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

  try {
    const session = await apiRequest('/Auth/dang-nhap', {
      method: 'POST',
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
    await new Promise(resolve => setTimeout(resolve, 360))
    await router.push(session.vaiTro === 'Admin' ? '/admin' : '/employee')
  } catch (error) {
    errorMessage.value = error.message || 'Dang nhap that bai'
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600;700&display=swap');

.login-section {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100dvh;
  width: 100%;
  padding: 24px;
  background: no-repeat;
  background-position: center -48px;
  background-size: cover;
  font-family: 'Poppins', sans-serif;
  opacity: 0;
  transform: translateY(-24px);
  transition:
    opacity 0.72s ease,
    transform 0.82s cubic-bezier(0.16, 1, 0.3, 1),
    background-position 1.15s cubic-bezier(0.16, 1, 0.3, 1),
    background-size 1.15s cubic-bezier(0.16, 1, 0.3, 1);
}

.login-section.is-ready {
  opacity: 1;
  transform: translateY(0);
  background-position: center;
  background-size: cover;
}

.form-box {
  position: relative;
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
  margin: 30px 0;
  width: 100%;
  min-height: 52px;
  border-bottom: 2px solid #fff;
}

.inputbox input::placeholder {
  color: rgba(255, 255, 255, 0.7);
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
  padding: 8px 35px 8px 5px;
  margin: 0;
  color: #fff;
  -webkit-appearance: none;
  appearance: none;
  transform: translateY(0);
  transition: color 0.2s ease;
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

.error-text {
  margin: 14px 0 0;
  color: #fee2e2;
  text-align: center;
  font-size: 0.92rem;
  text-shadow: 0 1px 6px rgba(0,0,0,0.45);
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
    background-position: center;
    background-size: cover;
  }

  .form-box {
    width: min(100%, 360px);
    min-height: auto;
    padding: 26px 0 28px;
    border-radius: 18px;
    backdrop-filter: blur(3px);
    -webkit-backdrop-filter: blur(3px);
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
    margin: 18px 0;
    min-height: 48px;
  }

  .inputbox input {
    height: 42px;
    font-size: 0.98rem;
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
