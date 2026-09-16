<template>
  <header class="h-16 bg-white border-b border-slate-200 flex items-center justify-between px-6 shrink-0">
    <h1 v-if="title" class="text-lg font-bold text-slate-800">{{ title }}</h1>
    <div v-else></div>

    <div class="flex items-center gap-3 relative">
      <div class="text-right">
        <p class="text-sm font-semibold">{{ worker.fullName }}</p>
        <p class="text-xs text-slate-400">{{ worker.userType }}</p>
      </div>

      <!-- Profile Button -->
      <button
        @click="showAccountMenu = !showAccountMenu"
        class="w-9 h-9 bg-emerald-700 rounded-full flex items-center justify-center text-white text-sm font-bold hover:bg-emerald-800 transition"
      >
        {{ initials }}
      </button>

      <!-- Account Menu -->
      <div
        v-if="showAccountMenu"
        class="absolute right-0 top-12 w-56 bg-white border border-slate-200 rounded-xl shadow-lg z-50 overflow-hidden"
      >
        <div class="px-4 py-3 border-b border-slate-100">
          <p class="text-sm font-semibold text-slate-800">
            {{ worker.fullName }}
          </p>
          <p class="text-xs text-slate-400">
            {{ worker.userType }}
          </p>
        </div>

        <button
          @click="openAvailabilityModal"
          class="w-full px-4 py-3 text-left text-sm text-slate-700 hover:bg-slate-50 transition"
        >
          Account Settings
        </button>
      </div>
    </div>

    <!-- Availability Modal -->
    <div
      v-if="showAvailabilityModal"
      class="fixed inset-0 bg-black/30 flex items-center justify-center z-[60]"
      @click.self="closeAvailabilityModal"
    >
      <div class="w-full max-w-sm bg-white rounded-xl shadow-xl border border-slate-200 p-6">
        <div class="flex items-start justify-between mb-5">
          <div>
            <h2 class="text-lg font-bold text-slate-800">
              Availability
            </h2>
            <p class="text-sm text-slate-500 mt-1">
              Set your current availability.
            </p>
          </div>

          <button
            @click="closeAvailabilityModal"
            class="text-slate-400 hover:text-slate-600 text-xl"
          >
            ×
          </button>
        </div>

        <div class="space-y-2">
          <!-- Available -->
          <button
            @click="selectedAvailability = 'Available'"
            class="w-full flex items-center gap-3 p-3 rounded-lg border transition"
            :class="
              selectedAvailability === 'Available'
                ? 'border-emerald-500 bg-emerald-50'
                : 'border-slate-200 hover:bg-slate-50'
            "
          >
            <span class="w-3 h-3 rounded-full bg-emerald-500"></span>

            <div class="text-left">
              <p class="text-sm font-semibold text-slate-800">
                Available
              </p>
              <p class="text-xs text-slate-500">
                Ready to receive patients
              </p>
            </div>
          </button>

          <!-- On Break -->
          <button
            @click="selectedAvailability = 'On Break'"
            class="w-full flex items-center gap-3 p-3 rounded-lg border transition"
            :class="
              selectedAvailability === 'On Break'
                ? 'border-amber-500 bg-amber-50'
                : 'border-slate-200 hover:bg-slate-50'
            "
          >
            <span class="w-3 h-3 rounded-full bg-amber-500"></span>

            <div class="text-left">
              <p class="text-sm font-semibold text-slate-800">
                On Break
              </p>
              <p class="text-xs text-slate-500">
                Temporarily unavailable
              </p>
            </div>
          </button>

          <!-- Offline -->
          <button
            @click="selectedAvailability = 'Offline'"
            class="w-full flex items-center gap-3 p-3 rounded-lg border transition"
            :class="
              selectedAvailability === 'Offline'
                ? 'border-slate-500 bg-slate-50'
                : 'border-slate-200 hover:bg-slate-50'
            "
          >
            <span class="w-3 h-3 rounded-full bg-slate-400"></span>

            <div class="text-left">
              <p class="text-sm font-semibold text-slate-800">
                Offline
              </p>
              <p class="text-xs text-slate-500">
                Not available for patients
              </p>
            </div>
          </button>
        </div>

        <!-- Error -->
        <p
          v-if="errorMessage"
          class="text-sm text-red-600 mt-4"
        >
          {{ errorMessage }}
        </p>

        <!-- Buttons -->
        <div class="flex justify-end gap-2 mt-6">
          <button
            @click="closeAvailabilityModal"
            class="px-4 py-2 text-sm font-medium text-slate-600 border border-slate-200 rounded-lg hover:bg-slate-50"
          >
            Cancel
          </button>

          <button
            @click="saveAvailability"
            :disabled="isSaving"
            class="px-4 py-2 text-sm font-medium text-white bg-emerald-700 rounded-lg hover:bg-emerald-800 disabled:opacity-50"
          >
            {{ isSaving ? 'Saving...' : 'Save' }}
          </button>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup>
import { computed, ref } from 'vue'

const props = defineProps({
  worker: {
    // { fullName, userType }
    type: Object,
    required: true,
  },
  title: {
    type: String,
    default: '',
  },
})

const initials = computed(() =>
  (props.worker.fullName || '')
    .split(' ')
    .filter(Boolean)
    .map(n => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2)
)

// =====================================================
// ACCOUNT MENU
// =====================================================

const showAccountMenu = ref(false)

// =====================================================
// AVAILABILITY
// =====================================================

const showAvailabilityModal = ref(false)
const selectedAvailability = ref('Offline')
const isSaving = ref(false)
const errorMessage = ref('')

const API_BASE = `${(
  import.meta.env.VITE_API_URL || 'http://localhost:57147'
).replace(/\/$/, '')}/api`

function authHeaders() {
  return {
    Authorization: `Bearer ${localStorage.getItem('aruga_token')}`,
    'Content-Type': 'application/json',
  }
}

function openAvailabilityModal() {
  showAccountMenu.value = false
  errorMessage.value = ''

  const stored = localStorage.getItem('aruga_user')

  if (stored) {
    try {
      const user = JSON.parse(stored)

      selectedAvailability.value =
        user.AvailabilityStatus ||
        user.availabilityStatus ||
        'Offline'
    } catch {
      selectedAvailability.value = 'Offline'
    }
  }

  showAvailabilityModal.value = true
}

function closeAvailabilityModal() {
  if (isSaving.value) return

  showAvailabilityModal.value = false
  errorMessage.value = ''
}

async function saveAvailability() {
  try {
    isSaving.value = true
    errorMessage.value = ''

    const response = await fetch(
      `${API_BASE}/Healthcare/availability`,
      {
        method: 'PUT',
        headers: authHeaders(),
        body: JSON.stringify({
          availabilityStatus: selectedAvailability.value,
        }),
      }
    )

    const data = await response.json()

    if (!response.ok) {
      throw new Error(
        data.message || 'Failed to update availability.'
      )
    }

    // Keep local user information updated
    const stored = localStorage.getItem('aruga_user')

    if (stored) {
      const user = JSON.parse(stored)

      user.AvailabilityStatus = selectedAvailability.value

      localStorage.setItem(
        'aruga_user',
        JSON.stringify(user)
      )
    }

    showAvailabilityModal.value = false

  } catch (error) {
    console.error('Availability update failed:', error)

    errorMessage.value =
      error.message || 'Failed to update availability.'

  } finally {
    isSaving.value = false
  }
}
</script>