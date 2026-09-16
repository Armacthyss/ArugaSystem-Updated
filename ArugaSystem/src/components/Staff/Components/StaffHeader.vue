<script setup>
import { computed } from "vue"
import { Bell, Settings } from "lucide-vue-next"

const account = computed(() => {
  try {
    return JSON.parse(localStorage.getItem("account") || "null")
  } catch {
    return null
  }
})

const displayName = computed(() => {
  const user = account.value

  if (!user) return "Staff"

  const name = `${user.firstName || ""} ${user.lastName || ""}`.trim()

  return name || user.username || "Staff"
})

const initials = computed(() => {
  const user = account.value

  if (!user) return "ST"

  const result = `${user.firstName?.charAt(0) || ""}${user.lastName?.charAt(0) || ""}`

  return result.toUpperCase() || "ST"
})
</script>

<template>
  <header
    class="flex items-center justify-between
           px-8 py-5 border-b border-stone-200 bg-white"
  >

    <!-- Page title -->
    <div>
      <h1 class="text-[22px] font-bold">
        Staff Dashboard
      </h1>

      <p class="text-[12.5px] mt-0.5 text-stone-500">
        Aruga / Dashboard
      </p>
    </div>

    <!-- Right side -->
    <div class="flex items-center gap-3">

      <!-- Notifications -->
      <button
        class="relative flex h-9 w-9 items-center
               justify-center rounded-full
               bg-stone-50 hover:bg-stone-100"
        title="Notifications"
      >
        <Bell
          :size="17"
          class="text-stone-500"
        />

        <span
          class="absolute top-1.5 right-2
                 h-1.5 w-1.5 rounded-full
                 bg-rose-600"
        />
      </button>

      <!-- Settings -->
      <button
        class="flex h-9 w-9 items-center
               justify-center rounded-full
               bg-stone-50 hover:bg-stone-100"
        title="Settings"
      >
        <Settings
          :size="17"
          class="text-stone-500"
        />
      </button>

      <!-- User -->
      <div
        class="flex h-9 w-9 items-center
               justify-center rounded-full
               text-white text-xs font-semibold
               bg-emerald-700"
        :title="displayName"
      >
        {{ initials }}
      </div>

    </div>

  </header>
</template>