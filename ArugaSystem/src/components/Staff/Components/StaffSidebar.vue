<script setup>
import { useRouter } from 'vue-router'
import {
  LogOut,
  Home,
  Users,
  Syringe,
  Package,
  ListChecks,
  Bell,
  BarChart3,
  Settings
} from 'lucide-vue-next'

import { logout } from '@/utils/auth'

const router = useRouter()

const navItems = [
  {
    id: 'home',
    label: 'Dashboard',
    path: '/staff/dashboard',
    icon: Home
  },
  {
    id: 'patients',
    label: 'Patient Records',
    path: '/staff/patient-records',
    icon: Users
  },
  {
    id: 'schedule',
    label: 'Vaccine Schedule',
    path: '/staff/vaccine-schedule',
    icon: Syringe
  },
  {
    id: 'inventory',
    label: 'Inventory',
    path: '/staff/inventory',
    icon: Package
  },
  {
    id: 'queue',
    label: 'Queue Management',
    path: '/staff/queue-management',
    icon: ListChecks
  },
  {
    id: 'notifications',
    label: 'Notifications',
    path: '/staff/notifications',
    icon: Bell
  },
  {
    id: 'reports',
    label: 'Clinic Reports',
    path: '/staff/reports',
    icon: BarChart3
  },
  {
    id: 'settings',
    label: 'Settings',
    path: '/staff/settings',
    icon: Settings
  }
]

function handleLogout() {
  logout()
  router.push('/')
}
</script>

<template>
 <aside class="w-64 h-screen bg-white border-r border-slate-200 flex flex-col shrink-0 sticky top-0">

    <!-- Logo -->
    <div class="p-6">
      <div class="flex items-center gap-3 px-2">

        <div
          class="w-10 h-10 bg-emerald-600 rounded-xl
                 flex items-center justify-center
                 text-white font-bold"
        >
          A
        </div>

        <div>
          <span class="text-lg font-bold tracking-tight">
            Aruga Pediatric System
          </span>

          <p class="text-xs text-slate-400">
            Staff Portal
          </p>
        </div>

      </div>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 px-3 space-y-1 overflow-y-auto">

      <button
        v-for="item in navItems"
        :key="item.id"
        @click="router.push(item.path)"
        class="w-full flex items-center gap-3
               px-4 py-2.5 rounded-lg
               text-sm font-medium
               transition-colors cursor-pointer"
        :class="
          $route.path === item.path
            ? 'bg-emerald-50 text-emerald-700'
            : 'text-slate-500 hover:bg-slate-50 hover:text-slate-900'
        "
      >

        <component
          :is="item.icon"
          class="w-4 h-4"
        />

        {{ item.label }}

      </button>

    </nav>
<!-- Logout -->
<div class="p-3 border-t border-slate-200">

      <button
        @click="handleLogout"
        class="w-full flex items-center gap-3
               px-4 py-2.5 rounded-lg
               text-sm font-medium
               text-slate-500
               hover:bg-rose-50
               hover:text-rose-700
               transition-colors
               cursor-pointer"
      >

        <LogOut class="w-4 h-4" />

        Logout

      </button>

    </div>

  </aside>
</template>