<script setup>
/* =========================================================================
   AppHeader
   Reusable top bar for the main content column. Each page just passes its
   own title/breadcrumb — everything else (notification bell, settings,
   avatar) stays identical across pages.
========================================================================= */

defineProps({
  title: { type: String, required: true },
  breadcrumb: { type: String, default: '' },
  userInitials: { type: String, default: 'RM' },
})

defineEmits(['notification-click', 'settings-click'])
</script>

<template>
  <header class="h-17.5 sticky top-0 z-20 bg-white border-b border-slate-200 flex items-center justify-between px-6 gap-4">
    <div class="min-w-0">
      <h1 class="text-lg font-bold text-slate-900 truncate">{{ title }}</h1>
      <p v-if="breadcrumb" class="text-xs text-slate-500 truncate">{{ breadcrumb }}</p>
    </div>
    <div class="flex items-center gap-3 shrink-0">
      <button
        @click="$emit('notification-click')"
        class="relative w-9 h-9 rounded-lg flex items-center justify-center text-slate-500 hover:bg-slate-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
      >
        <span aria-hidden="true">🔔</span>
        <span class="absolute top-1.5 right-1.5 w-2 h-2 rounded-full bg-rose-500 ring-2 ring-white"></span>
      </button>
      <button
        @click="$emit('settings-click')"
        class="w-9 h-9 rounded-lg flex items-center justify-center text-slate-500 hover:bg-slate-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
      >
        <span aria-hidden="true">⚙️</span>
      </button>
      <div class="w-9 h-9 rounded-full bg-emerald-100 text-emerald-700 flex items-center justify-center text-xs font-bold shrink-0">{{ userInitials }}</div>
    </div>
  </header>
</template>