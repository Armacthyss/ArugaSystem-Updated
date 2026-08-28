<script setup>
import { ref, computed } from "vue"
import { useRouter } from "vue-router"
import axios from "axios"

const API_BASE_URL = "http://localhost:57147/api/auth"

const router = useRouter()

// =====================================================
// FORM STATE 
// =====================================================

const identifier = ref("")
const password = ref("")
const showPassword = ref(false)
const rememberMe = ref(false)

const errorMessage = ref("")
const isLoading = ref(false)

const canSubmit = computed(() =>
  identifier.value.trim().length > 0 &&
  password.value.length > 0 &&
  !isLoading.value
)


// =====================================================
// ROLE REDIRECTION
// =====================================================

function redirectForRole(role) {
  switch (role) {
    case "Parent":
      router.push("/Dashboard")
      break

    // Doctor and Nurse share the same homepage in the current router.
    case "Doctor":
    case "Nurse":
      router.push({ name: "DoctorHome" })
      break

    case "Staff":
      router.push("/staff/dashboard")
      break

    case "Admin":
      router.push("/AdminHome")
      break

    case "SystemAdmin":
      router.push("/system-admin/home")
      break

    default:
      console.warn("Unknown role, no redirect configured:", role)
      errorMessage.value = "This account has no home page configured."
      break
  }
}


// =====================================================
// LOGIN
// =====================================================

async function handleLogin() {
  errorMessage.value = ""

  if (!canSubmit.value) {
    errorMessage.value = "Please enter your username or email and password."
    return
  }

  isLoading.value = true

  try {
    const response = await axios.post(`${API_BASE_URL}/login`, {
      identifier: identifier.value.trim(),
      password: password.value
    })

    const data = response.data

    // Persist session
    localStorage.setItem("authToken", data.token)
    localStorage.setItem("account", JSON.stringify(data))

    // Forced password change takes priority over role redirect
    if (data.mustChangePassword === true) {
      router.push("/ChangePassword")
      return
    }

    redirectForRole(data.role)

  } catch (error) {
    console.error("Login error:", error)

    if (error.response?.status === 401) {
      errorMessage.value =
        error.response?.data?.message || "Invalid username/email or password."
    } else if (error.response?.status === 400) {
      errorMessage.value =
        error.response?.data?.message || "Please check your login details."
    } else if (error.request) {
      errorMessage.value =
        "Can't reach the server. Please check your connection and try again."
    } else {
      errorMessage.value = "Something went wrong. Please try again."
    }
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex">
    <!-- LEFT SIDE -->
    <div
      class="hidden lg:flex lg:w-1/2 relative bg-cover bg-center"
      style="background-image: url('https://images.unsplash.com/photo-1584515933487-779824d29309');"
    >
      <div class="absolute inset-0 bg-linear-to-brom-green-900/80 via-green-800/75 to-green-700/85"></div>

      <div class="relative z-10 h-full w-full flex flex-col justify-between p-12 text-white">
        <div>
          <h1 class="text-3xl font-black tracking-wide">ARUGA</h1>
          <p class="text-sm text-white/70 mt-1">Pediatric Health Record System</p>
        </div>

        <div class="max-w-xl">
          <h2 class="text-5xl font-black leading-tight mb-6">Pediatric Immunization Tracker</h2>
          <p class="text-lg text-white/90 leading-relaxed">
            A centralized system for managing pediatric health records,
            vaccinations, appointments, and clinic operations.
          </p>
        </div>

        <div class="flex gap-6 text-sm text-white/70">
          <span>● Secure Access</span>
          <span>● Role-Based Access</span>
        </div>
      </div>
    </div>

    <!-- RIGHT SIDE -->
    <div class="w-full lg:w-1/2 bg-[#fff8ec] flex items-center justify-center px-6 py-10">
      <form class="w-full max-w-105 bg-white rounded-3xl shadow-xl p-10" @submit.prevent="handleLogin">
        <div class="mb-8">
          <h2 class="text-3xl font-black text-[#2d3a26]">Welcome to Aruga</h2>
          <p class="text-gray-500 mt-2">Sign in to continue to your account.</p>
        </div>

        <!-- USERNAME / EMAIL -->
        <div class="mb-5">
          <label for="identifier" class="block mb-2 text-sm font-semibold text-gray-700">
            Username or Email
          </label>
          <input
            id="identifier"
            v-model="identifier"
            type="text"
            autocomplete="username"
            placeholder="Enter your email or username"
            class="w-full border-2 border-[#dcccac] rounded-xl px-4 py-3 focus:outline-none focus:border-[#546b41]"
          />
        </div>

        <!-- PASSWORD -->
        <div class="mb-3">
          <label for="password" class="block mb-2 text-sm font-semibold text-gray-700">
            Password
          </label>
          <div class="relative">
            <input
              id="password"
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              autocomplete="current-password"
              placeholder="Enter your password"
              class="w-full border-2 border-[#dcccac] rounded-xl px-4 py-3 pr-12 focus:outline-none focus:border-[#546b41]"
            />
            <button
              type="button"
              class="absolute right-3 top-1/2 -translate-y-1/2 text-sm text-gray-400 hover:text-[#546b41]"
              :aria-label="showPassword ? 'Hide password' : 'Show password'"
              @click="showPassword = !showPassword"
            >
              {{ showPassword ? "🙈" : "👁️" }}
            </button>
          </div>
        </div>

        <!-- REMEMBER ME / FORGOT PASSWORD -->
        <div class="flex items-center justify-between text-sm mb-6">
          <label class="flex items-center gap-2 text-gray-600 cursor-pointer select-none">
            <input
              v-model="rememberMe"
              type="checkbox"
              class="rounded border-[#dcccac] text-[#546b41] focus:ring-[#546b41]"
            />
            Remember me
          </label>

          <button
            type="button"
            class="text-[#546b41] hover:underline"
            @click="router.push('/forgot-password')"
          >
            Forgot password?
          </button>
        </div>

        <!-- ERROR -->
        <div v-if="errorMessage" class="mb-4 rounded-xl bg-red-50 border border-red-200 px-4 py-3">
          <p class="text-red-600 text-sm">{{ errorMessage }}</p>
        </div>

        <!-- SUBMIT -->
        <button
          type="submit"
          :disabled="!canSubmit"
          class="w-full bg-[#546b41] hover:bg-[#455936] disabled:bg-gray-300 text-white py-3 rounded-xl font-bold transition-all"
        >
          <span v-if="!isLoading">Sign In</span>
          <span v-else>Signing in…</span>
        </button>

        <p class="text-center text-xs text-gray-400 mt-8 leading-relaxed">
          This login is used by parents, doctors, nurses, clinic staff,
          administrators, and system administrators.
        </p>
      </form>
    </div>
  </div>
</template>