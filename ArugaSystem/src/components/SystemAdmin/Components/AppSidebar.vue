<script setup>
import { ref } from 'vue'

/* =========================================================================
   AppSidebar
   Reusable across every page. Owns its own collapse state (it only affects
   its own width — the layout is a flex row, so the main column already
   fills the remaining space automatically).

   `activeNav` is a v-model: the parent page passes down which item is
   active (e.g. 'Inventory', 'Dashboard', ...) and listens for
   `update:activeNav` when the user clicks a different item. If you later
   wire up vue-router, you can drop the v-model and instead highlight based
   on the current route.
========================================================================= */

const props = defineProps({
  navItems: {
    type: Array,
    default: () => ([
      { label: 'Dashboard',          icon: '🏠', to: '/system-admin/home' },
      { label: 'User Management',    icon: '👥', to: '/system-admin/user-management' },
      { label: 'Patient Management', icon: '🧒', to: '/system-admin/patients' },
      { label: 'Vaccine Management', icon: '💉', to: '/system-admin/vaccines' },
      { label: 'Inventory',          icon: '📦', to: '/system-admin/inventory' },
      { label: 'Notifications',      icon: '🔔', to: '/system-admin/notifications' },
      { label: 'Reports',            icon: '📊', to: '/system-admin/reports' },
      { label: 'Audit Logs',         icon: '📋', to: '/system-admin/audit-logs' },
       { label: 'Settings',           icon: '⚙️', to: '/system-admin/operating-hours' },
      
      { label: 'Settings',           icon: '⚙️', to: '/system-admin/operating-hours' },
    ]),
  },
  activeNav: { type: String, default: '' },
  userName: { type: String, default: 'Renzo Miguel' },
  userRole: { type: String, default: 'System Admin' },
  userInitials: { type: String, default: 'RM' },
})

const emit = defineEmits(['update:activeNav', 'logout'])

const isCollapsed = ref(false)
const toggleSidebar = () => (isCollapsed.value = !isCollapsed.value)

function selectNav(label) {
  emit('update:activeNav', label)
}
</script>

<template>
  <aside
    :class="[isCollapsed ? 'w-20' : 'w-65']"
    class="hidden md:flex flex-col shrink-0 sticky top-0 h-screen bg-white border-r border-slate-200 transition-all duration-300 ease-in-out"
  >
    <div class="h-17.5 flex items-center gap-3 px-5 border-b border-slate-200 shrink-0">
      <div class="w-9 h-9 rounded-lg bg-emerald-600 flex items-center justify-center shrink-0">
        <span class="text-white font-bold text-sm">A</span>
      </div>
      <span v-if="!isCollapsed" class="font-bold text-slate-900 tracking-tight whitespace-nowrap overflow-hidden">Aruga</span>
    </div>

    <nav class="flex-1 overflow-y-auto py-4 px-3 space-y-1">
      <button
        v-for="item in navItems"
        :key="item.label"
        @click="selectNav(item.label)"
        :class="[activeNav === item.label ? 'bg-emerald-50 text-emerald-700' : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900']"
        class="w-full flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
      >
        <span class="text-base shrink-0" aria-hidden="true">{{ item.icon }}</span>
        <span v-if="!isCollapsed" class="truncate">{{ item.label }}</span>
      </button>
    </nav>

    <div class="border-t border-slate-200 p-3 shrink-0 space-y-2">
      <div class="flex items-center gap-3 px-2 py-2">
        <div class="w-9 h-9 rounded-full bg-emerald-100 text-emerald-700 flex items-center justify-center text-xs font-bold shrink-0">{{ userInitials }}</div>
        <div v-if="!isCollapsed" class="min-w-0">
          <p class="text-sm font-semibold text-slate-900 truncate">{{ userName }}</p>
          <p class="text-xs text-slate-500 truncate">{{ userRole }}</p>
        </div>
      </div>
      <button
        @click="emit('logout')"
        class="w-full flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-rose-600 hover:bg-rose-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-rose-400"
      >
        <span class="text-base shrink-0" aria-hidden="true">🚪</span>
        <span v-if="!isCollapsed">Log out</span>
      </button>
      <button @click="toggleSidebar" class="w-full flex items-center justify-center rounded-lg px-3 py-2 text-xs font-medium text-slate-400 hover:bg-slate-50 hover:text-slate-600 transition-colors">
        <span :class="isCollapsed ? 'rotate-180' : ''" class="transition-transform inline-block">◀</span>
      </button>
    </div>
  </aside>
</template>