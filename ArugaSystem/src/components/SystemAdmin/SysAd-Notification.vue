<script setup>
import { ref, computed, reactive, onMounted, watch } from 'vue'
import AppSidebar from './Components/AppSidebar.vue'
import AppHeader from './Components/AppHeader.vue'

const API_BASE = 'http://localhost:57147/api'
const authToken = () => localStorage.getItem('authToken')

/* -------------------------------- Status meta -------------------------------- */
const statusMeta = {
  Sent: { tint: 'bg-emerald-50', text: 'text-emerald-700', dot: 'bg-emerald-500' },
  Scheduled: { tint: 'bg-sky-50', text: 'text-sky-700', dot: 'bg-sky-500' },
  Pending: { tint: 'bg-amber-50', text: 'text-amber-700', dot: 'bg-amber-500' },
  Failed: { tint: 'bg-rose-50', text: 'text-rose-600', dot: 'bg-rose-500' },
}

const typeOptions = [
  'Vaccination Reminder',
  'Queue Notification',
  'Inventory Alert',
  'OTP / Account',
  'Manual Message',
]

/* -------------------------------- Notification data -------------------------------- */
const notifications = ref([])
const notificationsLoading = ref(false)
const notificationsError = ref('')

const formatDate = (value) => {
  if (!value) return '—'
  return new Date(value).toLocaleDateString('en-US', {
    month: 'short',
    day: '2-digit',
    year: 'numeric',
  })
}

const isToday = (value) => {
  if (!value) return false
  const date = new Date(value)
  const now = new Date()
  return (
    date.getFullYear() === now.getFullYear() &&
    date.getMonth() === now.getMonth() &&
    date.getDate() === now.getDate()
  )
}

const loadNotifications = async () => {
  notificationsLoading.value = true
  notificationsError.value = ''

  try {
    const response = await fetch(`${API_BASE}/Notifications/admin`, {
      headers: { Authorization: `Bearer ${authToken()}` },
    })

    if (!response.ok) throw new Error('Failed to load notifications.')

    const data = await response.json()

    notifications.value = data.map((n) => ({
      id: n.id,
      title: n.title,
      recipient: n.recipient,
      child: n.child,
      type: n.type,
      scheduledDate: formatDate(n.scheduledDate),
      sentDate: formatDate(n.sentDate),
      scheduledAt: n.scheduledDate,
      sentAt: n.sentDate,
      status: n.status,
      message: n.message,
      deliveryMethod: n.deliveryMethod,
      opened: n.opened,
      createdAt: n.createdAt,
    }))
  } catch (error) {
    console.error('Notification loading error:', error)
    notificationsError.value = 'Unable to load notification logs.'
  } finally {
    notificationsLoading.value = false
  }
}

const searchQuery = ref('')
const typeFilter = ref('All')
const statusFilter = ref('All')

const filteredNotifications = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()

  return notifications.value.filter((n) => {
    const matchesSearch =
      !query ||
      n.title.toLowerCase().includes(query) ||
      n.recipient.toLowerCase().includes(query) ||
      n.child.toLowerCase().includes(query)

    const matchesType = typeFilter.value === 'All' || n.type === typeFilter.value
    const matchesStatus = statusFilter.value === 'All' || n.status === statusFilter.value

    return matchesSearch && matchesType && matchesStatus
  })
})

const summary = computed(() => {
  const sentToday = notifications.value.filter((n) => n.sentAt && isToday(n.sentAt)).length
  const scheduled = notifications.value.filter((n) => n.status === 'Scheduled').length
  const pending = notifications.value.filter((n) => n.status === 'Pending').length
  const failed = notifications.value.filter((n) => n.status === 'Failed').length
  const manual = notifications.value.filter((n) => n.type === 'Manual Message').length

  const reminderNotifications = notifications.value.filter(
    (n) =>
      n.type === 'Vaccination Reminder' ||
      n.type === 'Upcoming Schedule' ||
      n.type === 'Missed Vaccination'
  )

  const successfulReminders = reminderNotifications.filter((n) => n.status === 'Sent').length

  const successRate =
    reminderNotifications.length > 0
      ? Math.round((successfulReminders / reminderNotifications.length) * 100)
      : 0

  return { sentToday, scheduled, pending, failed, manual, successRate }
})

/* ------------------------------ Pagination (table) ------------------------------ */
const currentPage = ref(1)
const itemsPerPage = ref(10)

watch([searchQuery, typeFilter, statusFilter, itemsPerPage], () => {
  currentPage.value = 1
})

const totalPages = computed(() =>
  Math.max(1, Math.ceil(filteredNotifications.value.length / itemsPerPage.value))
)

const paginatedNotifications = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage.value
  const end = start + itemsPerPage.value
  return filteredNotifications.value.slice(start, end)
})

const paginationStart = computed(() =>
  filteredNotifications.value.length === 0 ? 0 : (currentPage.value - 1) * itemsPerPage.value + 1
)

const paginationEnd = computed(() =>
  Math.min(currentPage.value * itemsPerPage.value, filteredNotifications.value.length)
)

const buildVisiblePages = (current, total) => {
  const pages = []

  if (total <= 5) {
    for (let i = 1; i <= total; i++) pages.push(i)
    return pages
  }

  pages.push(1)
  if (current > 3) pages.push('...')

  const start = Math.max(2, current - 1)
  const end = Math.min(total - 1, current + 1)
  for (let i = start; i <= end; i++) pages.push(i)

  if (current < total - 2) pages.push('...')
  pages.push(total)

  return pages
}

const visiblePages = computed(() => buildVisiblePages(currentPage.value, totalPages.value))

const goToPage = (page) => {
  if (page === '...') return
  currentPage.value = Math.min(Math.max(page, 1), totalPages.value)
}
const nextPage = () => {
  if (currentPage.value < totalPages.value) currentPage.value++
}
const previousPage = () => {
  if (currentPage.value > 1) currentPage.value--
}

/* ------------------------- Notification History pagination ------------------------- */
const historyCurrentPage = ref(1)
const historyItemsPerPage = ref(10)

const historyTotalPages = computed(() =>
  Math.max(1, Math.ceil(notifications.value.length / historyItemsPerPage.value))
)

const paginatedHistory = computed(() => {
  const start = (historyCurrentPage.value - 1) * historyItemsPerPage.value
  const end = start + historyItemsPerPage.value
  return notifications.value.slice(start, end)
})

const historyPaginationStart = computed(() =>
  notifications.value.length === 0
    ? 0
    : (historyCurrentPage.value - 1) * historyItemsPerPage.value + 1
)

const historyPaginationEnd = computed(() =>
  Math.min(historyCurrentPage.value * historyItemsPerPage.value, notifications.value.length)
)

const historyVisiblePages = computed(() =>
  buildVisiblePages(historyCurrentPage.value, historyTotalPages.value)
)

const goToHistoryPage = (page) => {
  if (page === '...') return
  historyCurrentPage.value = Math.min(Math.max(page, 1), historyTotalPages.value)
}
const nextHistoryPage = () => {
  if (historyCurrentPage.value < historyTotalPages.value) historyCurrentPage.value++
}
const previousHistoryPage = () => {
  if (historyCurrentPage.value > 1) historyCurrentPage.value--
}

watch([historyItemsPerPage, notifications], () => {
  historyCurrentPage.value = 1
})

/* ------------------------------ Row actions menu ---------------------------- */
const openMenuId = ref(null)
const toggleMenu = (id) => (openMenuId.value = openMenuId.value === id ? null : id)
const closeMenu = () => (openMenuId.value = null)

const resend = (n) => {
  // NOTE: local-only state change. Wire this up to a real
  // `POST ${API_BASE}/Notifications/{id}/resend` call once that
  // endpoint exists, then reload from the server instead of mutating locally.
  n.status = 'Sent'
  n.sentDate = formatDate(new Date())
  closeMenu()
}
const cancelScheduled = (n) => {
  n.status = 'Failed'
  closeMenu()
}
const deleteAnnouncement = (n) => {
  notifications.value = notifications.value.filter((x) => x.id !== n.id)
  closeMenu()
}

/* -------------------------------- Details drawer ---------------------------- */
const showDrawer = ref(false)
const selectedNotification = ref(null)
const openDrawer = (n) => {
  selectedNotification.value = n
  showDrawer.value = true
  closeMenu()
}
const closeDrawer = () => (showDrawer.value = false)

/* --------------------------------- New message modal --------------------------------- */
const showMessageModal = ref(false)

const messageForm = reactive({
  title: '',
  message: '',
  recipients: 'All Parents',
  schedule: 'Immediately',
  scheduleDate: '',
  method: 'In-App',
})

const messageSaving = ref(false)
const messageError = ref('')

const openMessageModal = () => {
  Object.assign(messageForm, {
    title: '',
    message: '',
    recipients: 'All Parents',
    schedule: 'Immediately',
    scheduleDate: '',
    method: 'In-App',
  })
  messageError.value = ''
  showMessageModal.value = true
}

const closeMessageModal = () => {
  if (messageSaving.value) return
  showMessageModal.value = false
  messageError.value = ''
}

const sendMessage = async () => {
  messageError.value = ''

  if (!messageForm.title.trim()) {
    messageError.value = 'Title is required.'
    return
  }
  if (!messageForm.message.trim()) {
    messageError.value = 'Message content is required.'
    return
  }
  if (messageForm.schedule === 'Later' && !messageForm.scheduleDate) {
    messageError.value = 'Please choose a schedule date/time.'
    return
  }

  messageSaving.value = true

  try {
    const response = await fetch(`${API_BASE}/Notifications`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${authToken()}`,
      },
      body: JSON.stringify({
        title: messageForm.title.trim(),
        message: messageForm.message.trim(),
        recipients: messageForm.recipients,
        deliveryMethod: messageForm.method,
        scheduledDate: messageForm.schedule === 'Later' ? messageForm.scheduleDate : null,
      }),
    })

    const data = await response.json().catch(() => ({}))

    if (!response.ok) {
      throw new Error(data.message || 'Failed to send message.')
    }

    await loadNotifications()
    closeMessageModal()
  } catch (error) {
    console.error('Send message error:', error)
    messageError.value = error.message || 'Unable to send message.'
  } finally {
    messageSaving.value = false
  }
}

/* --------------------------------- Notification rules --------------------------------- */
const notificationRules = ref([])
const rulesLoading = ref(false)
const rulesError = ref('')

const showRuleModal = ref(false)
const editingRuleId = ref(null)
const ruleSaving = ref(false)
const ruleDeleting = ref(false)
const ruleError = ref('')

const ruleForm = reactive({
  ruleName: '',
  notificationType: 'Vaccination',
  triggerType: 'BeforeDue',
  triggerValue: 7,
  inAppEnabled: true,
  smsEnabled: false,
  emailEnabled: false,
  isEnabled: true,
})

const loadNotificationRules = async () => {
  rulesLoading.value = true
  rulesError.value = ''

  try {
    const response = await fetch(`${API_BASE}/NotificationRules`, {
      headers: { Authorization: `Bearer ${authToken()}` },
    })

    if (!response.ok) throw new Error('Failed to load notification rules.')

    notificationRules.value = await response.json()
  } catch (error) {
    console.error('Notification rules loading error:', error)
    rulesError.value = 'Unable to load notification rules.'
  } finally {
    rulesLoading.value = false
  }
}

const resetRuleForm = () => {
  Object.assign(ruleForm, {
    ruleName: '',
    notificationType: 'Vaccination',
    triggerType: 'BeforeDue',
    triggerValue: 7,
    inAppEnabled: true,
    smsEnabled: false,
    emailEnabled: false,
    isEnabled: true,
  })
}

const openCreateRuleModal = () => {
  editingRuleId.value = null
  ruleError.value = ''
  resetRuleForm()
  showRuleModal.value = true
}

const openEditRuleModal = (rule) => {
  editingRuleId.value = rule.ruleID
  ruleError.value = ''

  Object.assign(ruleForm, {
    ruleName: rule.ruleName,
    notificationType: rule.notificationType,
    triggerType: rule.triggerType,
    triggerValue: rule.triggerValue,
    inAppEnabled: rule.inAppEnabled,
    smsEnabled: rule.smsEnabled,
    emailEnabled: rule.emailEnabled,
    isEnabled: rule.isEnabled,
  })

  showRuleModal.value = true
}

const closeRuleModal = () => {
  if (ruleSaving.value) return
  showRuleModal.value = false
  editingRuleId.value = null
  ruleError.value = ''
}

const saveNotificationRule = async () => {
  ruleError.value = ''

  if (!ruleForm.ruleName.trim()) {
    ruleError.value = 'Rule name is required.'
    return
  }
  if (!ruleForm.notificationType) {
    ruleError.value = 'Notification type is required.'
    return
  }
  if (!ruleForm.triggerType) {
    ruleError.value = 'Trigger type is required.'
    return
  }

  const triggerNeedsValue =
    ruleForm.triggerType === 'BeforeDue' || ruleForm.triggerType === 'MissedAfterDays'

  if (triggerNeedsValue) {
    if (ruleForm.triggerValue === null || ruleForm.triggerValue === '' || Number(ruleForm.triggerValue) < 1) {
      ruleError.value = 'Trigger value must be at least 1 day.'
      return
    }
  } else {
    ruleForm.triggerValue = null
  }

  ruleSaving.value = true

  try {
    const isEditing = !!editingRuleId.value

    const response = await fetch(
      isEditing ? `${API_BASE}/NotificationRules/${editingRuleId.value}` : `${API_BASE}/NotificationRules`,
      {
        method: isEditing ? 'PUT' : 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${authToken()}`,
        },
        body: JSON.stringify({
          ruleName: ruleForm.ruleName.trim(),
          notificationType: ruleForm.notificationType,
          triggerType: ruleForm.triggerType,
          triggerValue: ruleForm.triggerValue === '' ? null : ruleForm.triggerValue,
          inAppEnabled: ruleForm.inAppEnabled,
          smsEnabled: ruleForm.smsEnabled,
          emailEnabled: ruleForm.emailEnabled,
          isEnabled: ruleForm.isEnabled,
        }),
      }
    )

    const data = await response.json()

    if (!response.ok) throw new Error(data.message || 'Failed to save notification rule.')

    await loadNotificationRules()
    closeRuleModal()
  } catch (error) {
    console.error('Notification rule save error:', error)
    ruleError.value = error.message || 'Unable to save notification rule.'
  } finally {
    ruleSaving.value = false
  }
}

const deleteNotificationRule = async (rule) => {
  if (!window.confirm(`Delete the notification rule "${rule.ruleName}"?`)) return

  ruleDeleting.value = true
  rulesError.value = ''

  try {
    const response = await fetch(`${API_BASE}/NotificationRules/${rule.ruleID}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${authToken()}` },
    })

    const data = await response.json()

    if (!response.ok) throw new Error(data.message || 'Failed to delete notification rule.')

    await loadNotificationRules()
  } catch (error) {
    console.error('Notification rule delete error:', error)
    rulesError.value = error.message || 'Unable to delete notification rule.'
  } finally {
    ruleDeleting.value = false
  }
}

/* --------------------------------- Notification settings --------------------------------- */
const showSettingsModal = ref(false)

const settings = reactive({
  settingID: null,
  automaticNotificationsEnabled: true,
  defaultSendingTime: '08:00:00',
  inAppEnabled: true,
  smsEnabled: false,
  emailEnabled: false,
})

const settingsLoading = ref(false)
const settingsSaving = ref(false)
const settingsError = ref('')
const settingsSuccess = ref('')

const loadNotificationSettings = async () => {
  settingsLoading.value = true
  settingsError.value = ''

  try {
    const response = await fetch(`${API_BASE}/NotificationSettings`, {
      headers: { Authorization: `Bearer ${authToken()}` },
    })

    if (!response.ok) throw new Error('Failed to load notification settings.')

    const data = await response.json()
    Object.assign(settings, data)
  } catch (error) {
    console.error(error)
    settingsError.value = 'Unable to load notification settings.'
  } finally {
    settingsLoading.value = false
  }
}

const openSettingsModal = async () => {
  settingsSuccess.value = ''
  settingsError.value = ''
  showSettingsModal.value = true
  await loadNotificationSettings()
}

const saveNotificationSettings = async () => {
  settingsSaving.value = true
  settingsError.value = ''
  settingsSuccess.value = ''

  try {
    const response = await fetch(`${API_BASE}/NotificationSettings`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${authToken()}`,
      },
      body: JSON.stringify(settings),
    })

    if (!response.ok) throw new Error('Failed to save notification settings.')

    const data = await response.json()
    Object.assign(settings, data)

    settingsSuccess.value = 'Notification settings saved successfully.'

    setTimeout(() => {
      showSettingsModal.value = false
      settingsSuccess.value = ''
    }, 800)
  } catch (error) {
    console.error(error)
    settingsError.value = 'Unable to save notification settings.'
  } finally {
    settingsSaving.value = false
  }
}

onMounted(() => {
  loadNotificationSettings()
  loadNotificationRules()
  loadNotifications()
})
</script>

<template>
  <div class="min-h-screen bg-slate-50 flex text-slate-900" @click="closeMenu">
    <!-- ============================ SIDEBAR ============================ -->
    <AppSidebar />

    <!-- ============================ MAIN ============================ -->
    <div class="flex-1 min-w-0 flex flex-col">
      <AppHeader
        title="Notification Logs"
        breadcrumb="System Administration / Notification Logs"
        user-initials="RM"
      />

      <main class="p-6 space-y-6">
        <!-- Summary cards -->
        <section class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4">
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Sent Today</p>
              <div class="bg-emerald-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">📨</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.sentToday }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Scheduled</p>
              <div class="bg-sky-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">🗓️</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.scheduled }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Pending</p>
              <div class="bg-amber-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">⏳</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.pending }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Failed</p>
              <div class="bg-rose-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">⚠️</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.failed }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Manual Messages</p>
              <div class="bg-teal-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">📢</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.manual }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Reminder Success</p>
              <div class="bg-violet-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">📈</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.successRate }}%</p>
          </div>
        </section>

        <!-- Toolbar -->
        <section class="bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
          <div class="flex flex-col lg:flex-row lg:items-center gap-3">
            <div class="relative flex-1 min-w-0">
              <span class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-sm" aria-hidden="true">🔍</span>
              <input
                v-model="searchQuery"
                type="text"
                placeholder="Search by parent name, child name, or notification title..."
                class="w-full pl-9 pr-3 py-2 text-sm rounded-lg border border-slate-200 bg-slate-50 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
              />
            </div>

            <select
              v-model="typeFilter"
              class="text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-slate-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
            >
              <option value="All">All Types</option>
              <option v-for="t in typeOptions" :key="t">{{ t }}</option>
            </select>

            <select
              v-model="statusFilter"
              class="text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-slate-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
            >
              <option value="All">All Status</option>
              <option>Sent</option>
              <option>Scheduled</option>
              <option>Pending</option>
              <option>Failed</option>
            </select>

            <div class="flex items-center gap-2 shrink-0 flex-wrap">
              <button
                @click="openSettingsModal"
                class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
              >
                Notification Settings
              </button>
              <button class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500">
                Export Logs
              </button>
              <button
                @click="openMessageModal"
                class="text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
              >
                + New Message
              </button>
            </div>
          </div>
        </section>

        <!-- Notification Rules -->
        <section class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
          <div class="px-5 py-4 border-b border-slate-200">
            <div class="flex items-center justify-between gap-4">
              <div>
                <h2 class="text-sm font-bold text-slate-900">Notification Rules</h2>
                <p class="text-xs text-slate-500 mt-0.5">Configure when automatic notifications are generated.</p>
              </div>

              <button
                @click="openCreateRuleModal"
                class="text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors"
              >
                + Add Rule
              </button>
            </div>
          </div>

          <div v-if="rulesLoading" class="px-5 py-10 text-center text-sm text-slate-500">
            Loading notification rules...
          </div>

          <div v-else-if="rulesError" class="px-5 py-10 text-center text-sm text-rose-600">
            {{ rulesError }}
          </div>

          <div v-else-if="notificationRules.length === 0" class="px-5 py-10 text-center text-sm text-slate-400">
            No notification rules have been configured.
          </div>

          <div v-else class="divide-y divide-slate-100">
            <div
              v-for="rule in notificationRules"
              :key="rule.ruleID"
              class="px-5 py-4 flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4"
            >
              <div class="min-w-0">
                <div class="flex items-center gap-2">
                  <p class="text-sm font-semibold text-slate-900">{{ rule.ruleName }}</p>
                  <span
                    :class="rule.isEnabled ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-500'"
                    class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-semibold"
                  >
                    {{ rule.isEnabled ? 'Enabled' : 'Disabled' }}
                  </span>
                </div>

                <p class="text-xs text-slate-500 mt-1">
                  {{ rule.notificationType }} · {{ rule.triggerType }}
                  <span v-if="rule.triggerValue !== null">
                    · {{ rule.triggerValue }} day{{ rule.triggerValue === 1 ? '' : 's' }}
                  </span>
                </p>
              </div>

              <div class="flex items-center gap-2 shrink-0">
                <span
                  :class="rule.inAppEnabled ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-400'"
                  class="px-2.5 py-1 rounded-full text-xs font-semibold"
                >
                  In-App
                </span>
                <span
                  :class="rule.smsEnabled ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-400'"
                  class="px-2.5 py-1 rounded-full text-xs font-semibold"
                >
                  SMS
                </span>
                <span
                  :class="rule.emailEnabled ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-400'"
                  class="px-2.5 py-1 rounded-full text-xs font-semibold"
                >
                  Email
                </span>

                <button
                  @click="openEditRuleModal(rule)"
                  class="ml-2 text-xs font-semibold px-3 py-1.5 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50"
                >
                  Edit
                </button>
                <button
                  @click="deleteNotificationRule(rule)"
                  :disabled="ruleDeleting"
                  class="text-xs font-semibold px-3 py-1.5 rounded-lg border border-rose-200 text-rose-600 hover:bg-rose-50 disabled:opacity-50"
                >
                  Delete
                </button>
              </div>
            </div>
          </div>
        </section>

        <!-- Notification table -->
        <section class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
          <div class="overflow-x-auto">
            <table class="w-full text-sm">
              <thead>
                <tr class="border-b border-slate-200 bg-slate-50/60">
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-5 py-3">Notification Title</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Recipient</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Type</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Scheduled Date</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Sent Date</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Status</th>
                  <th class="text-right font-semibold text-slate-500 text-xs uppercase tracking-wide px-5 py-3">Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="notificationsLoading">
                  <td colspan="7" class="px-5 py-12 text-center text-sm text-slate-400">Loading notifications...</td>
                </tr>

                <tr v-else-if="notificationsError">
                  <td colspan="7" class="px-5 py-12 text-center text-sm text-rose-600">{{ notificationsError }}</td>
                </tr>

                <tr
                  v-else
                  v-for="n in paginatedNotifications"
                  :key="n.id"
                  class="border-b border-slate-100 last:border-0 hover:bg-slate-50 transition-colors"
                >
                  <td class="px-5 py-3">
                    <p class="font-semibold text-slate-900 whitespace-nowrap">{{ n.title }}</p>
                    <p class="text-xs text-slate-400 whitespace-nowrap">{{ n.child !== '—' ? n.child : 'Broadcast' }}</p>
                  </td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ n.recipient }}</td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ n.type }}</td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ n.scheduledDate }}</td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ n.sentDate }}</td>
                  <td class="px-3 py-3">
                    <span
                      :class="[statusMeta[n.status].tint, statusMeta[n.status].text]"
                      class="inline-flex items-center gap-1.5 text-xs font-semibold px-2.5 py-1 rounded-full whitespace-nowrap"
                    >
                      <span :class="statusMeta[n.status].dot" class="w-1.5 h-1.5 rounded-full"></span>
                      {{ n.status }}
                    </span>
                  </td>
                  <td class="px-5 py-3 text-right relative">
                    <button
                      @click.stop="toggleMenu(n.id)"
                      class="text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg w-8 h-8 inline-flex items-center justify-center transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
                    >
                      ⋮
                    </button>

                    <div
                      v-if="openMenuId === n.id"
                      @click.stop
                      class="absolute right-5 top-11 z-30 w-52 bg-white border border-slate-200 rounded-lg shadow-md py-1 text-left"
                    >
                      <button @click="openDrawer(n)" class="w-full text-left px-3.5 py-2 text-sm text-slate-700 hover:bg-slate-50 transition-colors">View Notification</button>
                      <button @click="resend(n)" class="w-full text-left px-3.5 py-2 text-sm text-slate-700 hover:bg-slate-50 transition-colors">Resend</button>
                      <button v-if="n.status === 'Scheduled'" @click="cancelScheduled(n)" class="w-full text-left px-3.5 py-2 text-sm text-slate-700 hover:bg-slate-50 transition-colors">Cancel Scheduled Notification</button>
                      <div class="my-1 border-t border-slate-100"></div>
                      <button v-if="n.type === 'Manual Message'" @click="deleteAnnouncement(n)" class="w-full text-left px-3.5 py-2 text-sm text-rose-600 hover:bg-rose-50 transition-colors">Delete Message</button>
                    </div>
                  </td>
                </tr>

                <tr v-if="!notificationsLoading && !notificationsError && filteredNotifications.length === 0">
                  <td colspan="7" class="px-5 py-12 text-center text-sm text-slate-400">No notifications match your search or filters.</td>
                </tr>
              </tbody>
            </table>

            <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 px-5 py-4 border-t border-slate-200">
              <div class="flex items-center gap-2">
                <span class="text-xs text-slate-500">
                  Showing <span class="font-semibold text-slate-700">{{ paginationStart }}</span>-<span class="font-semibold text-slate-700">{{ paginationEnd }}</span>
                  of <span class="font-semibold text-slate-700">{{ filteredNotifications.length }}</span>
                </span>

                <span class="text-xs text-slate-500 ml-2">Show</span>
                <input
                  v-model.number="itemsPerPage"
                  type="number"
                  min="1"
                  max="100"
                  class="w-16 text-xs rounded-lg border border-slate-200 bg-white px-2.5 py-1.5 text-slate-600 text-center focus:outline-none focus:ring-2 focus:ring-emerald-500"
                />
                <span class="text-xs text-slate-400">items</span>
              </div>

              <div class="flex items-center gap-1">
                <button
                  @click="previousPage"
                  :disabled="currentPage === 1"
                  class="px-3 py-1.5 text-xs font-semibold rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  Previous
                </button>

                <button
                  v-for="page in visiblePages"
                  :key="page"
                  @click="goToPage(page)"
                  :disabled="page === '...'"
                  :class="page === currentPage ? 'bg-emerald-600 text-white border-emerald-600' : 'border-slate-200 text-slate-600 hover:bg-slate-50'"
                  class="min-w-8 px-2 py-1.5 text-xs font-semibold rounded-lg border disabled:cursor-default"
                >
                  {{ page }}
                </button>

                <button
                  @click="nextPage"
                  :disabled="currentPage === totalPages"
                  class="px-3 py-1.5 text-xs font-semibold rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  Next
                </button>
              </div>
            </div>
          </div>
        </section>

        <!-- Notification History -->
        <section class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
          <div class="px-5 py-4 border-b border-slate-200">
            <h2 class="text-sm font-bold text-slate-900">Notification History</h2>
            <p class="text-xs text-slate-500 mt-0.5">Full delivery history, including read/opened status.</p>
          </div>

          <div class="overflow-x-auto">
            <table class="w-full text-sm">
              <thead>
                <tr class="border-b border-slate-200 bg-slate-50/60">
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-5 py-3">Notification Title</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Recipient</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Date</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Status</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-5 py-3">Opened</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="n in paginatedHistory"
                  :key="n.id"
                  class="border-b border-slate-100 last:border-0 hover:bg-slate-50 transition-colors"
                >
                  <td class="px-5 py-3">
                    <p class="font-semibold text-slate-900 whitespace-nowrap">{{ n.title }}</p>
                    <p class="text-xs text-slate-400 whitespace-nowrap">{{ n.child !== '—' ? n.child : 'Broadcast' }}</p>
                  </td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ n.recipient }}</td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">
                    {{ n.sentDate !== '—' ? n.sentDate : n.scheduledDate }}
                  </td>
                  <td class="px-3 py-3">
                    <span
                      :class="[statusMeta[n.status].tint, statusMeta[n.status].text]"
                      class="inline-flex items-center gap-1.5 text-xs font-semibold px-2.5 py-1 rounded-full whitespace-nowrap"
                    >
                      <span :class="statusMeta[n.status].dot" class="w-1.5 h-1.5 rounded-full"></span>
                      {{ n.status }}
                    </span>
                  </td>
                  <td class="px-5 py-3">
                    <span :class="n.opened ? 'text-emerald-700' : 'text-slate-400'" class="text-xs font-semibold">
                      {{ n.opened ? 'Yes' : 'No' }}
                    </span>
                  </td>
                </tr>

                <tr v-if="notifications.length === 0">
                  <td colspan="5" class="px-5 py-12 text-center text-sm text-slate-400">No notification history yet.</td>
                </tr>
              </tbody>
            </table>

            <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 px-5 py-4 border-t border-slate-200">
              <div class="flex items-center gap-2">
                <span class="text-xs text-slate-500">
                  Showing <span class="font-semibold text-slate-700">{{ historyPaginationStart }}</span>-<span class="font-semibold text-slate-700">{{ historyPaginationEnd }}</span>
                  of <span class="font-semibold text-slate-700">{{ notifications.length }}</span>
                </span>

                <span class="text-xs text-slate-500 ml-2">Show</span>
                <input
                  v-model.number="historyItemsPerPage"
                  type="number"
                  min="1"
                  max="100"
                  class="w-16 text-xs rounded-lg border border-slate-200 bg-white px-2.5 py-1.5 text-slate-600 text-center focus:outline-none focus:ring-2 focus:ring-emerald-500"
                />
                <span class="text-xs text-slate-400">items</span>
              </div>

              <div class="flex items-center gap-1">
                <button
                  @click="previousHistoryPage"
                  :disabled="historyCurrentPage === 1"
                  class="px-3 py-1.5 text-xs font-semibold rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  Previous
                </button>

                <button
                  v-for="page in historyVisiblePages"
                  :key="page"
                  @click="goToHistoryPage(page)"
                  :disabled="page === '...'"
                  :class="page === historyCurrentPage ? 'bg-emerald-600 text-white border-emerald-600' : 'border-slate-200 text-slate-600 hover:bg-slate-50'"
                  class="min-w-8 px-2 py-1.5 text-xs font-semibold rounded-lg border disabled:cursor-default"
                >
                  {{ page }}
                </button>

                <button
                  @click="nextHistoryPage"
                  :disabled="historyCurrentPage === historyTotalPages"
                  class="px-3 py-1.5 text-xs font-semibold rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  Next
                </button>
              </div>
            </div>
          </div>
        </section>
      </main>
    </div>

    <!-- ============================ NOTIFICATION DETAILS DRAWER ============================ -->
    <transition name="fade">
      <div v-if="showDrawer" class="fixed inset-0 bg-slate-900/30 z-40" @click="closeDrawer"></div>
    </transition>
    <transition name="slide">
      <aside v-if="showDrawer" class="fixed top-0 right-0 h-screen w-full max-w-sm bg-white border-l border-slate-200 shadow-lg z-50 flex flex-col">
        <div class="h-[70px] flex items-center justify-between px-5 border-b border-slate-200 shrink-0">
          <h2 class="text-sm font-bold text-slate-900">Notification Details</h2>
          <button @click="closeDrawer" class="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-50 transition-colors">✕</button>
        </div>

        <div v-if="selectedNotification" class="flex-1 overflow-y-auto p-6 space-y-6">
          <div>
            <span
              :class="[statusMeta[selectedNotification.status].tint, statusMeta[selectedNotification.status].text]"
              class="inline-flex items-center gap-1.5 text-xs font-semibold px-2.5 py-1 rounded-full mb-3"
            >
              <span :class="statusMeta[selectedNotification.status].dot" class="w-1.5 h-1.5 rounded-full"></span>
              {{ selectedNotification.status }}
            </span>
            <p class="text-base font-bold text-slate-900">{{ selectedNotification.title }}</p>
          </div>

          <div>
            <h3 class="text-xs font-bold uppercase tracking-wide text-slate-500 mb-2">Message Content</h3>
            <p class="text-sm text-slate-700 leading-relaxed bg-slate-50 rounded-lg p-4">{{ selectedNotification.message }}</p>
          </div>

          <div class="bg-slate-50 rounded-lg divide-y divide-slate-200">
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Recipient</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedNotification.recipient }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Child</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedNotification.child }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Notification Type</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedNotification.type }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Scheduled Date</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedNotification.scheduledDate }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Sent Date</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedNotification.sentDate }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Delivery Method</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedNotification.deliveryMethod }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Opened</span>
              <span :class="selectedNotification.opened ? 'text-emerald-700' : 'text-slate-400'" class="text-sm font-semibold">
                {{ selectedNotification.opened ? 'Yes' : 'No' }}
              </span>
            </div>
          </div>
        </div>

        <div class="border-t border-slate-200 p-4 flex items-center gap-2 shrink-0">
          <button @click="resend(selectedNotification)" class="flex-1 text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors">Resend</button>
          <button @click="closeDrawer" class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors">Close</button>
        </div>
      </aside>
    </transition>

    <!-- ============================ NEW MESSAGE MODAL ============================ -->
    <transition name="fade">
      <div
        v-if="showMessageModal"
        class="fixed inset-0 bg-slate-900/40 z-40 flex items-center justify-center p-4"
        @click.self="closeMessageModal"
      >
        <div class="bg-white rounded-xl shadow-lg w-full max-w-2xl max-h-[90vh] overflow-y-auto">
          <div class="flex items-center justify-between px-6 py-4 border-b border-slate-200">
            <h2 class="text-base font-bold text-slate-900">New Message</h2>
            <button @click="closeMessageModal" class="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-50 transition-colors">✕</button>
          </div>

          <div class="p-6 space-y-4">
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Title</label>
              <input
                v-model="messageForm.title"
                type="text"
                class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
              />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Message</label>
              <textarea
                v-model="messageForm.message"
                rows="4"
                class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors resize-none"
              ></textarea>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-semibold text-slate-500 mb-1.5">Recipients</label>
                <select
                  v-model="messageForm.recipients"
                  class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
                >
                  <option>All Parents</option>
                  <option>Selected Parents</option>
                </select>
              </div>
              <div>
                <label class="block text-xs font-semibold text-slate-500 mb-1.5">Notification Method</label>
                <select
                  v-model="messageForm.method"
                  class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
                >
                  <option>In-App</option>
                  <option disabled>SMS (Future)</option>
                  <option disabled>Email (Future)</option>
                </select>
              </div>
            </div>

            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Schedule</label>
              <div class="grid grid-cols-2 gap-2 mb-3">
                <button
                  @click="messageForm.schedule = 'Immediately'"
                  :class="messageForm.schedule === 'Immediately' ? 'bg-emerald-600 text-white' : 'border border-slate-200 text-slate-600 hover:bg-slate-50'"
                  class="text-sm font-semibold px-3 py-2 rounded-lg transition-colors"
                >
                  Immediately
                </button>
                <button
                  @click="messageForm.schedule = 'Later'"
                  :class="messageForm.schedule === 'Later' ? 'bg-emerald-600 text-white' : 'border border-slate-200 text-slate-600 hover:bg-slate-50'"
                  class="text-sm font-semibold px-3 py-2 rounded-lg transition-colors"
                >
                  Later
                </button>
              </div>
              <input
                v-if="messageForm.schedule === 'Later'"
                v-model="messageForm.scheduleDate"
                type="datetime-local"
                class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
              />
            </div>

            <div v-if="messageError" class="rounded-lg bg-rose-50 border border-rose-200 px-4 py-3 text-sm text-rose-700">
              {{ messageError }}
            </div>
          </div>

          <div class="flex items-center justify-end gap-2 px-6 py-4 border-t border-slate-200">
            <button @click="closeMessageModal" class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors">Cancel</button>
            <button
              @click="sendMessage"
              :disabled="messageSaving"
              class="text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {{ messageSaving ? 'Sending...' : 'Send' }}
            </button>
          </div>
        </div>
      </div>
    </transition>

    <!-- ============================ NOTIFICATION RULE MODAL (create/edit) ============================ -->
    <transition name="fade">
      <div
        v-if="showRuleModal"
        class="fixed inset-0 bg-slate-900/40 z-40 flex items-center justify-center p-4"
        @click.self="closeRuleModal"
      >
        <div class="bg-white rounded-xl shadow-lg w-full max-w-lg max-h-[90vh] overflow-y-auto">
          <div class="flex items-center justify-between px-6 py-4 border-b border-slate-200">
            <h2 class="text-base font-bold text-slate-900">
              {{ editingRuleId ? 'Edit Notification Rule' : 'New Notification Rule' }}
            </h2>
            <button @click="closeRuleModal" class="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-50 transition-colors">✕</button>
          </div>

          <div class="p-6 space-y-4">
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Rule Name</label>
              <input
                v-model="ruleForm.ruleName"
                type="text"
                class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
              />
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-semibold text-slate-500 mb-1.5">Notification Type</label>
                <select
                  v-model="ruleForm.notificationType"
                  class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
                >
                  <option value="Vaccination">Vaccination</option>
                  <option value="Queue">Queue</option>
                  <option value="Inventory">Inventory</option>
                </select>
              </div>
              <div>
                <label class="block text-xs font-semibold text-slate-500 mb-1.5">Trigger Type</label>
                <select
                  v-model="ruleForm.triggerType"
                  class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
                >
                  <option value="BeforeDue">Before Due</option>
                  <option value="MissedAfterDays">Missed After Days</option>
                  <option value="Immediate">Immediate</option>
                </select>
              </div>
            </div>

            <div v-if="ruleForm.triggerType === 'BeforeDue' || ruleForm.triggerType === 'MissedAfterDays'">
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Trigger Value (days)</label>
              <input
                v-model.number="ruleForm.triggerValue"
                type="number"
                min="1"
                class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
              />
            </div>

            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Channels</label>
              <div class="space-y-2">
                <label class="flex items-center justify-between gap-4 bg-slate-50 rounded-lg px-4 py-3 cursor-pointer">
                  <span class="text-sm font-medium text-slate-700">In-App</span>
                  <input v-model="ruleForm.inAppEnabled" type="checkbox" class="w-4 h-4 rounded border-slate-300 text-emerald-600 focus:ring-emerald-500" />
                </label>
                <label class="flex items-center justify-between gap-4 bg-slate-50 rounded-lg px-4 py-3 cursor-pointer">
                  <span class="text-sm font-medium text-slate-700">SMS</span>
                  <input v-model="ruleForm.smsEnabled" type="checkbox" class="w-4 h-4 rounded border-slate-300 text-emerald-600 focus:ring-emerald-500" />
                </label>
                <label class="flex items-center justify-between gap-4 bg-slate-50 rounded-lg px-4 py-3 cursor-pointer">
                  <span class="text-sm font-medium text-slate-700">Email</span>
                  <input v-model="ruleForm.emailEnabled" type="checkbox" class="w-4 h-4 rounded border-slate-300 text-emerald-600 focus:ring-emerald-500" />
                </label>
              </div>
            </div>

            <label class="flex items-center justify-between gap-4 bg-slate-50 rounded-lg px-4 py-3 cursor-pointer">
              <span class="text-sm font-medium text-slate-700">Rule Enabled</span>
              <input v-model="ruleForm.isEnabled" type="checkbox" class="w-4 h-4 rounded border-slate-300 text-emerald-600 focus:ring-emerald-500" />
            </label>

            <div v-if="ruleError" class="rounded-lg bg-rose-50 border border-rose-200 px-4 py-3 text-sm text-rose-700">
              {{ ruleError }}
            </div>
          </div>

          <div class="flex items-center justify-end gap-2 px-6 py-4 border-t border-slate-200">
            <button @click="closeRuleModal" class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors">Cancel</button>
            <button
              @click="saveNotificationRule"
              :disabled="ruleSaving"
              class="text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {{ ruleSaving ? 'Saving...' : 'Save Rule' }}
            </button>
          </div>
        </div>
      </div>
    </transition>

    <!-- ============================ NOTIFICATION SETTINGS MODAL ============================ -->
    <transition name="fade">
      <div
        v-if="showSettingsModal"
        class="fixed inset-0 bg-slate-900/40 z-40 flex items-center justify-center p-4"
        @click.self="showSettingsModal = false"
      >
        <div class="bg-white rounded-xl shadow-lg w-full max-w-lg max-h-[90vh] overflow-y-auto">
          <div class="flex items-center justify-between px-6 py-4 border-b border-slate-200">
            <h2 class="text-base font-bold text-slate-900">Notification Settings</h2>
            <button @click="showSettingsModal = false" class="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-50 transition-colors">✕</button>
          </div>

          <div class="p-6 space-y-6">
            <div v-if="settingsLoading" class="py-8 text-center text-sm text-slate-500">
              Loading notification settings...
            </div>

            <template v-else>
              <div>
                <div class="flex items-center justify-between gap-4">
                  <div>
                    <h3 class="text-sm font-semibold text-slate-900">Automatic Notifications</h3>
                    <p class="text-xs text-slate-500 mt-1">Enable or disable automatic notification processing.</p>
                  </div>

                  <label class="relative inline-flex items-center cursor-pointer shrink-0">
                    <input v-model="settings.automaticNotificationsEnabled" type="checkbox" class="sr-only peer" />
                    <div class="w-10 h-6 bg-slate-200 rounded-full peer peer-checked:bg-emerald-600 transition-colors">
                      <div class="w-4 h-4 bg-white rounded-full shadow-sm absolute left-1 top-1 peer-checked:translate-x-4 transition-transform"></div>
                    </div>
                  </label>
                </div>
              </div>

              <div>
                <label class="block text-sm font-semibold text-slate-900 mb-1.5">Default Sending Time</label>
                <p class="text-xs text-slate-500 mb-3">Automatic notifications will be processed starting at this time.</p>
                <input
                  v-model="settings.defaultSendingTime"
                  type="time"
                  class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
                />
              </div>

              <div>
                <h3 class="text-sm font-semibold text-slate-900">Notification Channels</h3>
                <p class="text-xs text-slate-500 mt-1 mb-3">Disable a channel if its external provider is unavailable or has insufficient credits.</p>

                <div class="space-y-2">
                  <label class="flex items-center justify-between gap-4 bg-slate-50 rounded-lg px-4 py-3 cursor-pointer">
                    <div>
                      <p class="text-sm font-medium text-slate-700">In-App Notifications</p>
                      <p class="text-xs text-slate-400">Aruga internal notifications</p>
                    </div>
                    <input v-model="settings.inAppEnabled" type="checkbox" class="w-4 h-4 rounded border-slate-300 text-emerald-600 focus:ring-emerald-500" />
                  </label>

                  <label class="flex items-center justify-between gap-4 bg-slate-50 rounded-lg px-4 py-3 cursor-pointer">
                    <div>
                      <p class="text-sm font-medium text-slate-700">SMS Notifications</p>
                      <p class="text-xs text-slate-400">TextBee</p>
                    </div>
                    <input v-model="settings.smsEnabled" type="checkbox" class="w-4 h-4 rounded border-slate-300 text-emerald-600 focus:ring-emerald-500" />
                  </label>

                  <label class="flex items-center justify-between gap-4 bg-slate-50 rounded-lg px-4 py-3 cursor-pointer">
                    <div>
                      <p class="text-sm font-medium text-slate-700">Email Notifications</p>
                      <p class="text-xs text-slate-400">EmailJS</p>
                    </div>
                    <input v-model="settings.emailEnabled" type="checkbox" class="w-4 h-4 rounded border-slate-300 text-emerald-600 focus:ring-emerald-500" />
                  </label>
                </div>
              </div>

              <div v-if="settingsError" class="rounded-lg bg-rose-50 border border-rose-200 px-4 py-3 text-sm text-rose-700">
                {{ settingsError }}
              </div>

              <div v-if="settingsSuccess" class="rounded-lg bg-emerald-50 border border-emerald-200 px-4 py-3 text-sm text-emerald-700">
                {{ settingsSuccess }}
              </div>
            </template>
          </div>

          <div class="flex items-center justify-end gap-2 px-6 py-4 border-t border-slate-200">
            <button @click="showSettingsModal = false" class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors">Cancel</button>
            <button
              @click="saveNotificationSettings"
              :disabled="settingsSaving || settingsLoading"
              class="text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {{ settingsSaving ? 'Saving...' : 'Save Settings' }}
            </button>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
.slide-enter-active, .slide-leave-active { transition: transform 0.25s ease; }
.slide-enter-from, .slide-leave-to { transform: translateX(100%); }

@media (prefers-reduced-motion: reduce) {
  * { transition-duration: 0.01ms !important; }
}
</style>