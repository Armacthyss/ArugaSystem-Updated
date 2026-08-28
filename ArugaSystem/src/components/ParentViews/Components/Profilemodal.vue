<template>
  <Transition name="modal-fade" appear>
    <div class="fixed inset-0 z-300 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm" @click.self="$emit('close')">
      <div class="relative w-full max-w-2xl bg-white rounded-4xl shadow-2xl overflow-hidden max-h-[90vh] flex flex-col">
        <div class="bg-[#5d6b52] px-8 py-7 flex items-center gap-5 shrink-0">
          <div class="w-16 h-16 rounded-2xl bg-white/20 flex items-center justify-center text-4xl shadow-lg shrink-0">👤</div>
          <div class="flex-1 min-w-0">
            <p class="text-white font-black text-xl leading-tight">
              {{ parentData?.firstName }} {{ parentData?.middleName ? parentData.middleName + ' ' : '' }}{{ parentData?.lastName }}
            </p>
            <p class="text-[#c8d9b0] text-xs font-bold mt-1">Parent/Guardian Account · Aruga Pediatric Portal</p>
          </div>
          <button @click="$emit('close')" class="w-9 h-9 rounded-xl bg-white/10 hover:bg-white/25 flex items-center justify-center text-white text-lg transition-all shrink-0">✕</button>
        </div>
        <div class="overflow-y-auto flex-1 p-8 space-y-8">
          <div>
            <p class="text-[10px] font-black text-[#99ad7a] uppercase tracking-[0.2em] mb-4">Your Information</p>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div class="bg-slate-50 rounded-2xl px-5 py-4"><p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Email Address</p><p class="text-sm font-bold text-slate-800 break-all">{{ parentData?.email || '—' }}</p></div>
              <div class="bg-slate-50 rounded-2xl px-5 py-4"><p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Contact Number</p><p class="text-sm font-bold text-slate-800">{{ parentData?.contactNo || parentData?.contactNumber || '—' }}</p></div>
              <div class="bg-slate-50 rounded-2xl px-5 py-4"><p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Barangay</p><p class="text-sm font-bold text-slate-800">{{ parentData?.barangayNo || parentData?.barangay || '—' }}</p></div>
              <div class="bg-slate-50 rounded-2xl px-5 py-4"><p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Address</p><p class="text-sm font-bold text-slate-800">{{ parentData?.address || '—' }}</p></div>
            </div>
          </div>

          <!-- CHANGE PASSWORD -->
          <div>
            <button @click="showChangePw = !showChangePw"
              class="w-full flex items-center justify-between px-5 py-4 bg-slate-50 hover:bg-slate-100 rounded-2xl transition-colors text-left group">
              <div class="flex items-center gap-3">
                <div class="w-9 h-9 rounded-xl bg-[#5d6b52]/10 flex items-center justify-center text-lg">🔒</div>
                <div>
                  <p class="text-sm font-black text-slate-800">Change Password</p>
                  <p class="text-[10px] text-slate-400">Update your account password</p>
                </div>
              </div>
              <span class="text-slate-400 text-xs font-bold transition-transform"
                :class="showChangePw ? 'rotate-180' : ''">▼</span>
            </button>
            <div>
              <div class="flex items-center justify-between mb-4">
                <p class="text-[10px] font-black text-[#99ad7a] uppercase tracking-[0.2em]">Children</p>
                <span class="text-[9px] font-black text-white bg-[#5d6b52] px-2.5 py-1 rounded-full">{{ children.length }} registered</span>
              </div>

              <div v-if="showChangePw" class="mt-3 space-y-3 px-1">
                <!-- Current Password -->
                <div>
                  <label class="text-[10px] font-black text-slate-400 uppercase tracking-widest block mb-1.5">
                    Current Password
                  </label>
                  <input v-model="pwForm.current" type="password"
                    placeholder="Enter your current password"
                    class="w-full px-4 py-3 text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#5d6b52] focus:border-transparent transition-all" />
                </div>

                <!-- New Password -->
                <div>
                  <label class="text-[10px] font-black text-slate-400 uppercase tracking-widest block mb-1.5">
                    New Password
                  </label>
                  <input v-model="pwForm.newPw" type="password"
                    placeholder="At least 6 characters"
                    class="w-full px-4 py-3 text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#5d6b52] focus:border-transparent transition-all" />
                </div>

                <!-- Confirm Password -->
                <div>
                  <label class="text-[10px] font-black text-slate-400 uppercase tracking-widest block mb-1.5">
                    Confirm New Password
                  </label>
                  <input v-model="pwForm.confirm" type="password"
                    placeholder="Re-enter new password"
                    class="w-full px-4 py-3 text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#5d6b52] focus:border-transparent transition-all" />
                </div>

                <!-- Error / Success -->
                <p v-if="pwError"   class="text-xs text-red-500 font-bold px-1">⚠ {{ pwError }}</p>
                <p v-if="pwSuccess" class="text-xs text-emerald-600 font-bold px-1">{{ pwSuccess }}</p>

                <!-- Submit -->
                <button @click="changePassword" :disabled="pwLoading"
                  class="w-full py-3 bg-[#5d6b52] hover:bg-[#4a5741] disabled:opacity-50 disabled:cursor-not-allowed text-white rounded-xl text-[11px] font-black uppercase tracking-widest transition-colors flex items-center justify-center gap-2">
                  <span v-if="pwLoading">Updating…</span>
                  <span v-else>🔒 Update Password</span>
                </button>

                <p class="text-[9px] text-slate-400 text-center font-bold px-2">
                  After changing your password, you'll need to log in again next time.
                </p>
              </div>
            </div>
            <div v-if="children.length === 0" class="text-center py-8 text-slate-400"><p class="text-2xl mb-2">👶</p><p class="text-sm font-bold">No children registered yet.</p></div>
            <div v-else class="space-y-3">
              <div v-for="child in children" :key="child.childID" class="border border-slate-100 rounded-2xl overflow-hidden">
                <button @click="toggleChildCard(child.childID)" class="w-full flex items-center gap-4 px-5 py-4 bg-slate-50 hover:bg-slate-100 transition-colors text-left">
                  <div class="w-10 h-10 rounded-xl bg-[#5d6b52]/10 flex items-center justify-center text-xl shrink-0">{{ child.sex === 'Female' ? '👧' : '👶' }}</div>
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-black text-slate-800 leading-tight">{{ child.firstName }} {{ child.lastName }}</p>
                    <p class="text-[10px] text-slate-400 mt-0.5">{{ formatDisplayDate(new Date(child.birthDate)) }} · {{ child.sex || '—' }}</p>
                  </div>
                  <span class="text-slate-400 text-xs font-bold">{{ expandedChildren.has(child.childID) ? '▲' : '▼' }}</span>
                </button>
                <div v-if="expandedChildren.has(child.childID)" class="px-5 py-4 grid grid-cols-1 sm:grid-cols-3 gap-3 bg-white border-t border-slate-50">
                  <div class="bg-slate-50 rounded-xl px-4 py-3"><p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Mother's Name</p><p class="text-sm font-bold text-slate-700">{{ child.motherName || '—' }}</p></div>
                  <div class="bg-slate-50 rounded-xl px-4 py-3"><p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Father's Name</p><p class="text-sm font-bold text-slate-700">{{ child.fatherName || '—' }}</p></div>
                  <div class="bg-slate-50 rounded-xl px-4 py-3"><p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Legal Guardian</p><p class="text-sm font-bold text-slate-700">{{ child.guardianName || '—' }}</p></div>
                  <div class="bg-slate-50 rounded-xl px-4 py-3"><p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Place of Birth</p><p class="text-sm font-bold text-slate-700">{{ child.placeOfBirth || '—' }}</p></div>
                  <div class="bg-slate-50 rounded-xl px-4 py-3"><p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Patient ID</p><p class="text-sm font-mono font-bold text-slate-700">#{{ child.childID.substring(0, 8).toUpperCase() }}</p></div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="shrink-0 px-8 py-5 border-t border-slate-100 bg-[#f8f9f5] flex items-center justify-between gap-4">
          <p class="text-[10px] text-slate-400 font-bold">To update your information, please contact the clinic staff.</p>
          <button @click="$emit('close')" class="bg-[#5d6b52] hover:bg-[#4a5741] text-white px-6 py-2.5 rounded-xl text-[10px] font-black uppercase tracking-widest transition-colors shrink-0">Close</button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { ref } from 'vue'
import axios from 'axios'
import { format } from 'date-fns'

const API_BASE_URL = 'http://localhost:57147'

// parentData and children are fetched/owned by whichever page renders this
// component (see HARD RULE 3) and passed down as props. The page controls
// visibility with v-if="showProfile", so this component just emits 'close'.
const props = defineProps({
  parentData: { type: Object, default: null },
  children: { type: Array, default: () => [] },
})

defineEmits(['close'])

function formatDisplayDate(date) {
  if (!date) return '—'
  try { return format(new Date(date), 'MMM d, yyyy') } catch { return '—' }
}

// ── Children accordion ──────────────────────────────────────────────────
const expandedChildren = ref(new Set())

function toggleChildCard(childId) {
  const next = new Set(expandedChildren.value)
  next.has(childId) ? next.delete(childId) : next.add(childId)
  expandedChildren.value = next
}

// ── Change Password ──────────────────────────────────────────────────────
const showChangePw = ref(false)
const pwForm    = ref({ current: '', newPw: '', confirm: '' })
const pwError   = ref('')
const pwSuccess = ref('')
const pwLoading = ref(false)

async function changePassword() {
  pwError.value   = ''
  pwSuccess.value = ''
  if (!pwForm.value.current)          { pwError.value = 'Please enter your current password.'; return }
  if (pwForm.value.newPw.length < 6)  { pwError.value = 'New password must be at least 6 characters.'; return }
  if (pwForm.value.newPw !== pwForm.value.confirm) { pwError.value = 'Passwords do not match.'; return }

  pwLoading.value = true
  try {
    await axios.patch(`${API_BASE_URL}/api/Parents/${props.parentData.parentID}/change-password`, {
      currentPassword: pwForm.value.current,
      newPassword:     pwForm.value.newPw,
    })
    pwSuccess.value = '✓ Password changed successfully!'
    pwForm.value    = { current: '', newPw: '', confirm: '' }
    setTimeout(() => { pwSuccess.value = ''; showChangePw.value = false }, 2500)
  } catch (err) {
    pwError.value = err.response?.data?.message || 'Current password is incorrect.'
  } finally {
    pwLoading.value = false
  }
}
</script>

<style scoped>
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
.modal-fade-enter-active > div, .modal-fade-leave-active > div { transition: transform 0.25s cubic-bezier(0.4,0,0.2,1); }
.modal-fade-enter-from > div, .modal-fade-leave-to > div { transform: scale(0.95); }
</style>