                                                                                        <template>
                                                                                          <div class="w-full min-h-screen bg-[#FDFCF7] flex justify-center font-sans antialiased text-slate-900">
                                                                                            <div class="w-full max-w-312.5 px-6 py-6">

                                                                                              <!-- 1. TOP HEADER -->
                                                                                              <header class="relative z-100 flex justify-between items-center mb-6">
                                                                                                <div class="flex items-center gap-3">
                                                                                                  <div class="w-10 h-10 bg-[#5d6b52] rounded-xl flex items-center justify-center text-white font-black shadow-lg">A</div>
                                                                                                  <div>
                                                                                                    <h1 class="text-lg font-bold text-[#2d3a26] leading-none">Aruga</h1>
                                                                                                    <p class="text-[9px] font-black text-[#99ad7a] uppercase tracking-tighter">Pediatric Portal</p>
                                                                                                  </div>
                                                                                                </div>

                                                                                                <div class="flex items-center gap-4">
                                                                                                  <button @click="showNotifications = true" class="relative w-10 h-10 rounded-full bg-white border border-slate-200 flex items-center justify-center shadow-sm hover:bg-slate-50 transition-colors">
                                                                                                    <span class="text-lg">🔔</span>
                                                                                                    <span v-if="unreadCount > 0" class="absolute -top-1 -right-1 bg-[#ff4d4d] text-white text-[8px] font-bold px-1.5 py-0.5 rounded-full min-w-4.5 text-center">
                                                                                                      {{ unreadCount > 99 ? '99+' : unreadCount }}
                                                                                                    </span>
                                                                                                  </button>

                                                                                                  <div class="flex items-center gap-3">
                                                                                                    <div class="hidden sm:flex flex-col items-end text-right cursor-pointer hover:opacity-70 transition-opacity" @click="showProfile = true">
                                                                                                      <span class="font-bold text-sm">{{ parentData?.firstName }} {{ parentData?.lastName }}</span>
                                                                                                      <span class="text-[9px] font-black uppercase text-[#99ad7a]">View Profile</span>
                                                                                                    </div>
                                                                                                    <div class="relative">
                                                                                                      <button @click.stop="toggleProfileMenu" class="w-10 h-10 rounded-full bg-white border border-slate-200 flex items-center justify-center shadow-sm hover:border-[#99ad7a] transition-all">
                                                                                                        <span class="text-sm">👤</span>
                                                                                                      </button>
                                                                                                      <div v-if="isProfileMenuOpen" class="absolute right-0 top-full mt-2 w-52 bg-white border border-slate-200 rounded-xl shadow-2xl overflow-hidden py-1 z-150">
                                                                                                        <div class="px-4 py-3 border-b border-slate-100">
                                                                                                          <p class="text-xs font-black text-[#2d3a26]">{{ parentData?.firstName }} {{ parentData?.lastName }}</p>
                                                                                                          <p class="text-[10px] text-slate-400 mt-0.5">Parent/Guardian Account</p>
                                                                                                        </div>
                                                                                                        <button @click.stop="showProfile = true; isProfileMenuOpen = false" class="w-full text-left px-4 py-3 text-sm text-slate-700 font-bold hover:bg-slate-50 flex items-center gap-2 transition-colors">
                                                                                                          <span>👤</span> View Profile
                                                                                                        </button>
                                                                                                        <button @click.stop="handleLogout" class="w-full text-left px-4 py-3 text-sm text-red-600 font-bold hover:bg-red-50 flex items-center gap-2 transition-colors">
                                                                                                          <span>🚪</span> Logout
                                                                                                        </button>
                                                                                                      </div>
                                                                                                    </div>
                                                                                                  </div>
                                                                                                </div>
                                                                                              </header>

                                                                                              <!-- 2. MAIN NAVIGATION -->
                                                                                              <nav class="flex items-center justify-between bg-white border border-slate-200/60 p-2 rounded-3xl shadow-sm mb-10 sticky top-4 z-50 backdrop-blur-md gap-3">
                                                                                                <div class="flex gap-1 shrink-0">
                                                                                                  <button v-for="tab in ['Overview', 'Check-in', 'Schedule', 'Records']" :key="tab"
                                                                                                    @click="activeNav = tab"
                                                                                                    :class="activeNav === tab ? 'bg-[#5d6b52] text-white shadow-md' : 'text-slate-400 hover:bg-slate-50'"
                                                                                                    class="px-5 py-2.5 rounded-[18px] text-[11px] font-black uppercase tracking-widest transition-all">
                                                                                                    {{ tab }}
                                                                                                  </button>
                                                                                                </div>
                                                                                                <div class="relative flex-1 max-w-sm">
                                                                                                  <input v-model="searchQuery" @focus="searchFocused = true" @blur="onSearchBlur"
                                                                                                        type="text" placeholder="Search children..."
                                                                                                        class="w-full bg-[#f3f4f1] border-none rounded-[18px] py-2.5 pl-10 pr-4 text-[11px] focus:ring-2 focus:ring-[#5d6b52] outline-none transition-all" />
                                                                                                  <span class="absolute left-4 top-2.5 text-xs">🔍</span>
                                                                                                  <div v-if="searchFocused && searchQuery.trim() && searchResults.length > 0"
                                                                                                      class="absolute top-full mt-2 left-0 right-0 bg-white rounded-[18px] border border-slate-100 shadow-xl overflow-hidden z-200">
                                                                                                    <button v-for="child in searchResults" :key="child.childID" @mousedown.prevent="selectFromSearch(child)"
                                                                                                            class="w-full px-4 py-3 flex items-center gap-3 hover:bg-slate-50 transition-colors text-left border-b border-slate-50 last:border-0">
                                                                                                      <span class="text-lg">{{ child.sex === 'Female' ? '👧' : '👶' }}</span>
                                                                                                      <div>
                                                                                                        <p class="text-xs font-bold text-slate-800">{{ child.firstName }} {{ child.lastName }}</p>
                                                                                                        <p class="text-[9px] text-slate-400 font-medium">ID: #{{ child.childID.substring(0, 8) }}</p>
                                                                                                      </div>
                                                                                                      <span class="ml-auto text-[9px] text-[#99ad7a] font-black uppercase">Select →</span>
                                                                                                    </button>
                                                                                                  </div>
                                                                                                  <div v-else-if="searchFocused && searchQuery.trim() && searchResults.length === 0"
                                                                                                      class="absolute top-full mt-2 left-0 right-0 bg-white rounded-[18px] border border-slate-100 shadow-xl px-4 py-4 text-center z-200">
                                                                                                    <p class="text-[10px] text-slate-400 font-bold">No children found for "{{ searchQuery }}"</p>
                                                                                                  </div>
                                                                                                </div>
                                                                                              </nav>

                                                                                              <div class="grid grid-cols-12 gap-8">

                                                                                                <!-- 3. SIDEBAR -->
                                                                                                <aside class="col-span-12 lg:col-span-3 space-y-4">
                                                                                                  <div class="flex items-center justify-between px-2">
                                                                                                    <h3 class="text-[10px] font-black text-[#99ad7a] uppercase tracking-[0.2em]">Family Profiles</h3>
                                                                                                    <span class="text-[9px] font-black text-white bg-[#5d6b52] px-2 py-0.5 rounded-full">{{ children.length }}</span>
                                                                                                  </div>
                                                                                                  <div class="space-y-2 max-h-[calc(100vh-220px)] overflow-y-auto pr-1">
                                                                                                    <button v-for="child in children" :key="child.childID"
                                                                                                      @click="selectChild(child)"
                                                                                                      :class="selectedChild?.childID === child.childID ? 'border-[#5d6b52] bg-white ring-4 ring-[#5d6b52]/5' : 'border-transparent bg-white/50 hover:bg-white'"
                                                                                                      class="w-full p-4 rounded-[28px] border-2 transition-all text-left flex items-center gap-3">
                                                                                                      <span class="text-xl shrink-0">{{ child.sex === 'Female' ? '👧' : '👶' }}</span>
                                                                                                      <div class="min-w-0">
                                                                                                        <p class="text-xs font-bold text-slate-800 leading-tight truncate">{{ child.firstName }} {{ child.lastName }}</p>
                                                                                                        <p class="text-[9px] text-slate-400 font-medium mt-0.5">ID: #{{ child.childID.substring(0, 8) }}</p>
                                                                                                      </div>
                                                                                                    </button>
                                                                                                  </div>
                                                                                                </aside>

                                                                                                <!-- 4. MAIN CONTENT -->
                                                                                                <main class="col-span-12 lg:col-span-9">

                                                                                                  <!-- OVERVIEW -->
                                                                                                  <div v-if="activeNav === 'Overview'" class="space-y-5 animate-in slide-in-from-bottom-4 duration-500">
                                                                                                    <div class="bg-white rounded-4xl border border-slate-100 shadow-sm p-8 flex flex-col md:flex-row justify-between items-center gap-6">
                                                                                                      <div>
                                                                                                        <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Today's Priority Ticket</p>
                                                                                                        <h2 class="text-4xl font-black text-[#2d3a26]">You are Queue <span class="text-[#99ad7a]">#015</span></h2>
                                                                                                        <p class="text-sm font-bold text-slate-500 mt-1">Active for {{ selectedChild ? `${selectedChild.firstName} ${selectedChild.lastName}` : 'Select a child' }}</p>
                                                                                                      </div>
                                                                                                      <div class="bg-[#f0f2ed] px-10 py-6 rounded-[28px] border border-[#99ad7a]/20 text-center min-w-45">
                                                                                                        <p class="text-[10px] font-black text-[#5d6b52] uppercase mb-1">Now Serving</p>
                                                                                                        <p class="text-5xl font-black text-[#2d3a26]">#012</p>
                                                                                                        <p class="text-[10px] font-bold text-[#5d6b52] mt-2 uppercase">3rd in line</p>
                                                                                                      </div>
                                                                                                    </div>

                                                                                                    <div v-if="selectedChild" class="bg-white rounded-4xl border border-slate-100 shadow-sm overflow-hidden">
                                                                                                      <div class="bg-[#5d6b52] px-8 py-5 flex items-center gap-5">
                                                                                                        <div class="w-14 h-14 rounded-2xl bg-white/20 flex items-center justify-center text-3xl shadow">{{ selectedChild.sex === 'Female' ? '👧' : '👶' }}</div>
                                                                                                        <div>
                                                                                                          <p class="text-white font-black text-lg leading-tight">{{ selectedChild.firstName }} {{ selectedChild.lastName }}</p>
                                                                                                          <p class="text-[#c8d9b0] text-[10px] font-bold uppercase mt-0.5">{{ selectedChild.healthCenter || 'Leveriza Health Center' }}</p>
                                                                                                        </div>
                                                                                                        <div class="ml-auto text-right hidden sm:block">
                                                                                                          <p class="text-[10px] text-white/60 font-black uppercase">Patient ID</p>
                                                                                                          <p class="text-white font-mono text-xs font-bold">#{{ selectedChild.childID.substring(0, 8).toUpperCase() }}</p>
                                                                                                        </div>
                                                                                                      </div>
                                                                                                      <div class="grid grid-cols-2 sm:grid-cols-3 divide-x divide-y divide-slate-50">
                                                                                                        <div class="px-6 py-5"><p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Date of Birth</p><p class="text-sm font-black text-slate-800">{{ formatDisplayDate(new Date(selectedChild.birthDate)) }}</p></div>
                                                                                                        <div class="px-6 py-5"><p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Age</p><p class="text-sm font-black text-slate-800">{{ childAge }}</p></div>
                                                                                                        <div class="px-6 py-5"><p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Sex</p><p class="text-sm font-black text-slate-800">{{ selectedChild.sex || '—' }}</p></div>
                                                                                                        <div class="px-6 py-5"><p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Birth Weight</p><p class="text-sm font-black text-slate-800">{{ selectedChild.birthWeight ? selectedChild.birthWeight + ' kg' : '—' }}</p></div>
                                                                                                        <div class="px-6 py-5"><p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Birth Height</p><p class="text-sm font-black text-slate-800">{{ selectedChild.birthHeight ? selectedChild.birthHeight + ' cm' : '—' }}</p></div>
                                                                                                        <div class="px-6 py-5">
                                                                                                          <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Barangay</p>
                                                                                                          <p class="text-sm font-black text-slate-800">{{ selectedChild.barangay || selectedChild.barangayNo || selectedChild.Barangay || '—' }}</p>
                                                                                                        </div>
                                                                                                      </div>
                                                                                                    </div>

                                                                                                    <div class="bg-white rounded-4xl border border-slate-100 p-7 shadow-sm">
                                                                                                      <div class="flex justify-between items-center mb-5">
                                                                                                        <h3 class="text-base font-black text-slate-800">Upcoming Doses</h3>
                                                                                                        <button @click="activeNav = 'Schedule'" class="text-[10px] font-black text-[#99ad7a] uppercase tracking-widest hover:text-[#5d6b52] transition-colors">View Schedule →</button>
                                                                                                      </div>
                                                                                                      <div class="space-y-3">
                                                                                                        <div v-for="vax in upcomingDoses.slice(0, 3)" :key="vax.doseId"
                                                                                                            class="p-4 bg-slate-50 rounded-2xl border border-slate-100 flex items-center justify-between">
                                                                                                          <div class="flex items-center gap-3">
                                                                                                            <div class="w-10 h-10 bg-[#5d6b52] rounded-xl flex items-center justify-center text-white text-sm">💉</div>
                                                                                                            <div>
                                                                                                              <p class="text-xs font-bold text-slate-800">{{ vax.name }} · Dose {{ vax.doseNumber }}</p>
                                                                                                              <p class="text-[10px] text-slate-400 mt-0.5">{{ formatDisplayDate(vax.scheduledDate) }}</p>
                                                                                                            </div>
                                                                                                          </div>
                                                                                                          <span class="px-3 py-1 bg-white rounded-lg text-[9px] font-black text-[#5d6b52] border border-slate-200 uppercase">Scheduled</span>
                                                                                                        </div>
                                                                                                        <p v-if="upcomingDoses.length > 3" class="text-center text-[10px] text-slate-400 font-bold pt-1">
                                                                                                          +{{ upcomingDoses.length - 3 }} more doses scheduled
                                                                                                        </p>
                                                                                                      </div>
                                                                                                    </div>
                                                                                                  </div>

                                                                                                  <!-- CHECK-IN -->
                                                                                                  <div v-else-if="activeNav === 'Check-in'" class="animate-in fade-in zoom-in-95 duration-500 space-y-4">
                                                                                                    <div v-if="selectedChild" class="bg-white rounded-[28px] border border-slate-100 shadow-sm p-5 flex items-center gap-5">
                                                                                                      <div class="w-12 h-12 rounded-2xl bg-[#f0f2ed] flex items-center justify-center text-2xl shrink-0">{{ selectedChild.sex === 'Female' ? '👧' : '👶' }}</div>
                                                                                                      <div class="flex-1 min-w-0">
                                                                                                        <p class="text-[9px] font-black text-[#99ad7a] uppercase tracking-widest mb-0.5">Checking in as</p>
                                                                                                        <p class="text-sm font-black text-[#2d3a26] truncate">{{ selectedChild.firstName }} {{ selectedChild.lastName }}</p>
                                                                                                        <p class="text-[10px] text-slate-400">{{ selectedChild.healthCenter || 'Leveriza Health Center' }}</p>
                                                                                                      </div>
                                                                                                      <div v-if="upcomingDoses.length > 0" class="text-right hidden sm:block shrink-0">
                                                                                                        <p class="text-[9px] font-black text-slate-400 uppercase mb-0.5">Next vaccine</p>
                                                                                                        <p class="text-[10px] font-black text-[#5d6b52]">{{ upcomingDoses[0]?.name }}</p>
                                                                                                        <p class="text-[9px] text-slate-400">Dose {{ upcomingDoses[0]?.doseNumber }} · {{ formatDisplayDate(upcomingDoses[0]?.scheduledDate) }}</p>
                                                                                                      </div>
                                                                                                    </div>
                                                                                                    <div v-else class="bg-[#FFF9E6] border border-[#F2E4B8] rounded-[28px] p-5 flex items-center gap-3">
                                                                                                      <span>⚠️</span>
                                                                                                      <p class="text-[11px] text-[#856404] font-bold">Please select a child from Family Profiles before checking in.</p>
                                                                                                    </div>
                                                                                                    <div class="bg-[#2d3a26] rounded-4xl p-10 text-center text-white relative overflow-hidden shadow-2xl">
                                                                                                      <div class="absolute inset-0 opacity-5" style="background-image: radial-gradient(circle, #99ad7a 1px, transparent 1px); background-size: 24px 24px;"></div>
                                                                                                      <div class="relative">
                                                                                                        <h2 class="text-2xl font-black mb-2">Scan QR Code</h2>
                                                                                                        <p class="text-white/50 text-xs mb-8">Align the clinic's QR code within the frame to check in</p>
                                                                                                        <div class="relative w-56 h-56 mx-auto mb-8">
                                                                                                          <div class="absolute inset-0 bg-black/50 rounded-2xl"></div>
                                                                                                          <div class="absolute inset-x-0 top-0 h-0.5 bg-linear-to-r from-transparent via-[#99ad7a] to-transparent shadow-[0_0_12px_#99ad7a]" style="animation: scanline 2s ease-in-out infinite;"></div>
                                                                                                          <div class="absolute top-2 left-2 w-6 h-6 border-t-2 border-l-2 border-[#99ad7a] rounded-tl-lg"></div>
                                                                                                          <div class="absolute top-2 right-2 w-6 h-6 border-t-2 border-r-2 border-[#99ad7a] rounded-tr-lg"></div>
                                                                                                          <div class="absolute bottom-2 left-2 w-6 h-6 border-b-2 border-l-2 border-[#99ad7a] rounded-bl-lg"></div>
                                                                                                          <div class="absolute bottom-2 right-2 w-6 h-6 border-b-2 border-r-2 border-[#99ad7a] rounded-br-lg"></div>
                                                                                                          <div class="absolute inset-0 flex items-center justify-center"><span class="text-4xl opacity-20">📷</span></div>
                                                                                                        </div>
                                                                                                        <div class="flex flex-col sm:flex-row gap-3 justify-center">
                                                                                                          <button class="px-10 py-3.5 bg-[#99ad7a] hover:bg-[#b5c99a] text-white rounded-2xl font-black text-xs uppercase tracking-widest transition-all shadow-lg">Allow Camera Access</button>
                                                                                                          <button @click="activeNav = 'Overview'" class="px-10 py-3.5 bg-white/10 hover:bg-white/20 text-white rounded-2xl font-black text-xs uppercase tracking-widest transition-all">Cancel</button>
                                                                                                        </div>
                                                                                                        <p class="text-white/30 text-[9px] mt-6 uppercase tracking-widest font-bold">Check-in is only available during clinic hours · Mon, Wed, Fri · 8:00 AM – 12:00 PM</p>
                                                                                                      </div>
                                                                                                    </div>
                                                                                                  </div>

                                                                                                  <!-- SCHEDULE -->
                                                                                                  <div v-else-if="activeNav === 'Schedule'" class="animate-in fade-in slide-in-from-right-4 duration-500">
                                                                                                    <div v-if="!selectedChild" class="bg-white rounded-[40px] p-12 text-center text-slate-400 border border-slate-100 shadow-sm">
                                                                                                      <p class="text-2xl mb-2">👶</p>
                                                                                                      <p class="font-bold text-sm">Select a child from Family Profiles to view their schedule.</p>
                                                                                                    </div>
                                                                                                    <div v-else class="flex gap-5 items-start">
                                                                                                      <div class="w-[42%] shrink-0 flex flex-col bg-white rounded-4xl border border-slate-100 shadow-sm overflow-hidden" style="max-height: 680px;">
                                                                                                        <div class="px-6 pt-6 pb-4 border-b border-slate-50">
                                                                                                          <p class="text-[10px] font-black text-[#99ad7a] uppercase tracking-[0.2em]">Vaccination Schedule</p>
                                                                                                          <p class="text-xs text-slate-400 mt-0.5">{{ selectedChild.firstName }} · {{ computedVaccineList.length }} doses</p>
                                                                                                        </div>
                                                                                                        <div class="overflow-y-auto flex-1 p-4 space-y-2">
                                                                                                          <template v-for="(group, gIdx) in groupedVaccineList" :key="gIdx">
                                                                                                            <p class="text-[9px] font-black text-slate-700 uppercase tracking-widest px-2 pt-3 pb-1 first:pt-0">{{ group.name }}</p>
                                                                                                            <button v-for="vax in group.doses" :key="vax.doseId" @click="selectedVax = vax"
                                                                                                              :class="selectedVax?.doseId === vax.doseId ? 'bg-[#5d6b52] text-white shadow-md' : 'bg-slate-50 hover:bg-slate-100 text-slate-700'"
                                                                                                              class="w-full px-4 py-3 rounded-2xl flex items-center justify-between transition-all text-left">
                                                                                                              <div class="flex items-center gap-3">
                                                                                                                <span :class="selectedVax?.doseId === vax.doseId ? 'bg-white/20' : vax.isCompleted ? 'bg-green-100' : 'bg-white'" class="w-7 h-7 rounded-lg flex items-center justify-center text-xs shadow-sm shrink-0">
                                                                                                                  {{ vax.isCompleted ? '✅' : '💉' }}
                                                                                                                </span>
                                                                                                                <div>
                                                                                                                  <p class="text-[11px] font-black leading-tight" :class="selectedVax?.doseId === vax.doseId ? 'text-white' : 'text-slate-700'">
                                                                                                                    Dose {{ vax.doseNumber }}
                                                                                                                    <span v-if="vax.isCompleted" class="ml-1 text-[8px] font-black text-green-600 bg-green-100 px-1.5 py-0.5 rounded-full uppercase">Taken</span>
                                                                                                                  </p>
                                                                                                                  <!-- FIX: show actual date administered vs original due date -->
                                                                                                                  <div v-if="vax.isCompleted && vax.wasLate" class="mt-0.5 space-y-0.5">
                                                                                                                    <p class="text-[9px]" :class="selectedVax?.doseId === vax.doseId ? 'text-white/50' : 'text-slate-400'">
                                                                                                                      Due: {{ formatDisplayDate(vax.originalDueDate) }}
                                                                                                                      <span class="text-orange-400 font-black ml-1">+{{ vax.daysLate }}d late</span>
                                                                                                                    </p>
                                                                                                                    <p class="text-[10px] font-bold" :class="selectedVax?.doseId === vax.doseId ? 'text-white/80' : 'text-slate-600'">Given: {{ formatDisplayDate(vax.administeredDate) }}</p>
                                                                                                                  </div>
                                                                                                                  <p v-else-if="vax.isCompleted" class="text-[10px] font-bold mt-0.5" :class="selectedVax?.doseId === vax.doseId ? 'text-white/80' : 'text-slate-600'">Given: {{ formatDisplayDate(vax.administeredDate) }}</p>
                                                                                                                  <p v-else class="text-[10px] mt-0.5" :class="selectedVax?.doseId === vax.doseId ? 'text-white/70' : 'text-slate-400'">{{ formatDisplayDate(vax.scheduledDate) }}</p>
                                                                                                                </div>
                                                                                                              </div>
                                                                                                              <span v-if="selectedVax?.doseId === vax.doseId" class="text-[9px] font-black text-white/80 uppercase tracking-wide shrink-0">Viewing →</span>
                                                                                                              <span v-else-if="vax.isCompleted" class="text-[9px] font-black text-green-600 uppercase shrink-0">✓ Done</span>
                                                                                                            </button>
                                                                                                          </template>
                                                                                                        </div>
                                                                                                      </div>
                                                                                                      <div class="flex-1 flex flex-col gap-4 sticky top-22">
                                                                                                        <div class="bg-white rounded-4xl border border-slate-100 shadow-sm p-7">
                                                                                                          <div v-if="selectedVax" class="flex items-center gap-3 mb-6 pb-5 border-b border-slate-50">
                                                                                                            <div class="w-9 h-9 bg-[#5d6b52] rounded-xl flex items-center justify-center text-white text-sm">💉</div>
                                                                                                            <div>
                                                                                                              <p class="text-sm font-black text-[#2d3a26] leading-tight">{{ selectedVax.name }}</p>
                                                                                                              <p class="text-[10px] text-[#99ad7a] font-black uppercase">Dose {{ selectedVax.doseNumber }}</p>
                                                                                                            </div>
                                                                                                            <span class="ml-auto text-[10px] font-black text-[#5d6b52] bg-[#5d6b52]/10 px-3 py-1.5 rounded-full uppercase">
                                                                                                              {{ selectedVax.isCompleted ? formatDisplayDate(selectedVax.administeredDate) : formatDisplayDate(selectedVax.scheduledDate) }}
                                                                                                            </span>
                                                                                                          </div>
                                                                                                          <div class="flex items-center justify-between mb-5">
                                                                                                            <button @click="prevMonth" class="w-8 h-8 rounded-xl bg-slate-50 hover:bg-slate-100 flex items-center justify-center text-slate-500 font-black transition-all text-sm">‹</button>
                                                                                                            <h3 class="font-black text-base text-[#2d3a26]">{{ calendarMonthLabel }}</h3>
                                                                                                            <button @click="nextMonth" class="w-8 h-8 rounded-xl bg-slate-50 hover:bg-slate-100 flex items-center justify-center text-slate-500 font-black transition-all text-sm">›</button>
                                                                                                          </div>
                                                                                                          <div class="grid grid-cols-7 gap-1 mb-2">
                                                                                                            <div v-for="(label, i) in ['S','M','T','W','T','F','S']" :key="i" class="text-center text-[9px] font-black text-slate-300 uppercase py-1">{{ label }}</div>
                                                                                                          </div>
                                                                                                          <div class="grid grid-cols-7 gap-1">
                                                                                                            <div v-for="n in calendarOffset" :key="'sp-' + n" class="aspect-square"></div>
                                                                                                            <div v-for="day in calendarDaysInMonth" :key="day" :class="getDayClass(day)" class="aspect-square flex items-center justify-center rounded-xl text-[11px] font-bold transition-all">{{ day }}</div>
                                                                                                          </div>
                                                                                                          <div class="mt-5 flex flex-wrap items-center justify-center gap-4 border-t border-slate-50 pt-5">
                                                                                                            <div class="flex items-center gap-1.5"><div class="w-2.5 h-2.5 rounded-full bg-[#5d6b52]"></div><span class="text-[9px] text-slate-400 font-black uppercase">Scheduled</span></div>
                                                                                                            <div class="flex items-center gap-1.5"><div class="w-2.5 h-2.5 rounded-full bg-green-500"></div><span class="text-[9px] text-slate-400 font-black uppercase">Given</span></div>
                                                                                                            <div class="flex items-center gap-1.5"><div class="w-2.5 h-2.5 rounded-full bg-red-400"></div><span class="text-[9px] text-slate-400 font-black uppercase">Missed Due Date</span></div>
                                                                                                            <div class="flex items-center gap-1.5"><div class="w-2.5 h-2.5 rounded-full bg-[#99ad7a]/40 border border-[#99ad7a]/40"></div><span class="text-[9px] text-slate-400 font-black uppercase">Window (MWF)</span></div>
                                                                                                            <div class="flex items-center gap-1.5"><div class="w-2.5 h-2.5 rounded-full bg-slate-100"></div><span class="text-[9px] text-slate-400 font-black uppercase">Clinic Day</span></div>
                                                                                                          </div>
                                                                                                        </div>
                                                                                                        <div class="space-y-2">
                                                                                                          <div class="bg-[#FFF9E6] border border-[#F2E4B8] rounded-2xl px-4 py-3 flex items-start gap-2">
                                                                                                            <span class="text-xs mt-0.5">💡</span>
                                                                                                            <p class="text-[10px] text-[#856404]"><span class="font-black">Clinic hours:</span> Mon, Wed, Fri · 8:00 AM – 12:00 PM</p>
                                                                                                          </div>
                                                                                                          <div class="bg-[#FFF2F2] border-l-4 border-[#FF4D4D] px-4 py-3 rounded-r-2xl flex items-center gap-2">
                                                                                                            <span class="text-[9px] font-black text-[#B32D2D] uppercase">⚠ Stocks may change without notice</span>
                                                                                                          </div>
                                                                                                          <div v-if="selectedVax && !selectedVax.isCompleted" class="bg-[#F2F7F2] border-l-4 border-[#99ad7a] px-4 py-3 rounded-r-2xl">
                                                                                                            <p class="text-[10px] text-[#5d6b52] font-black uppercase mb-0.5">Catch-up Window</p>
                                                                                                            <p class="text-[10px] text-slate-500">{{ formatDisplayDate(selectedVax.scheduledDate) }} <span class="text-slate-300 mx-1">→</span> {{ formatDisplayDate(windowEndDate) }}</p>
                                                                                                          </div>
                                                                                                        </div>
                                                                                                      </div>
                                                                                                    </div>
                                                                                                  </div>

                                                                                                  <!-- RECORDS -->
                                                                                                  <div v-else-if="activeNav === 'Records'" class="space-y-5 animate-in fade-in slide-in-from-right-4 duration-500">
                                                                                                    <div class="grid grid-cols-3 gap-4">
                                                                                                      <div class="bg-[#5d6b52] p-6 rounded-3xl text-center text-white shadow-sm"><h3 class="text-4xl font-black">{{ recordStats.completed }}</h3><p class="text-[9px] uppercase font-black opacity-70 mt-1 tracking-widest">Completed</p></div>
                                                                                                      <div class="bg-[#99ad7a] p-6 rounded-3xl text-center text-white shadow-sm"><h3 class="text-4xl font-black">{{ recordStats.scheduled }}</h3><p class="text-[9px] uppercase font-black opacity-70 mt-1 tracking-widest">Scheduled</p></div>
                                                                                                      <div class="bg-[#e57373] p-6 rounded-3xl text-center text-white shadow-sm"><h3 class="text-4xl font-black">{{ recordStats.overdue }}</h3><p class="text-[9px] uppercase font-black opacity-70 mt-1 tracking-widest">Overdue</p></div>
                                                                                                    </div>
                                                                                                    <div class="bg-white rounded-4xl border border-slate-100 shadow-sm overflow-hidden">
                                                                                                      <div class="px-8 py-6 border-b border-slate-50 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                                                                                                        <div>
                                                                                                          <h2 class="text-base font-black text-slate-800">Vaccination History</h2>
                                                                                                          <p class="text-[10px] text-slate-400 mt-0.5">{{ selectedChild?.firstName }} {{ selectedChild?.lastName }} <span class="text-slate-200 mx-1">·</span> {{ vaccinationHistory.length }} record{{ vaccinationHistory.length !== 1 ? 's' : '' }}</p>
                                                                                                        </div>
                                                                                                        <div class="flex gap-1 bg-slate-50 p-1 rounded-xl">
                                                                                                          <!-- FIX: filter values now match actual DB status strings -->
                                                                                                          <button v-for="f in ['All', 'Completed', 'Scheduled', 'Overdue']" :key="f" @click="recordFilter = f"
                                                                                                                  :class="recordFilter === f ? 'bg-white shadow-sm text-[#5d6b52] font-black' : 'text-slate-400 hover:text-slate-600'"
                                                                                                                  class="px-3 py-1.5 rounded-lg text-[9px] uppercase font-bold transition-all">{{ f }}</button>
                                                                                                        </div>
                                                                                                      </div>
                                                                                                      <div v-if="recordsLoading" class="py-16 text-center text-slate-400"><p class="text-2xl mb-2 animate-pulse">💉</p><p class="text-xs font-bold">Loading records...</p></div>
                                                                                                      <div v-else-if="filteredRecords.length === 0" class="py-16 text-center text-slate-400"><p class="text-2xl mb-2">📋</p><p class="text-xs font-bold">No {{ recordFilter === 'All' ? '' : recordFilter.toLowerCase() + ' ' }}vaccination records found.</p></div>
                                                                                                      <div v-else class="overflow-x-auto">
                                                                                                        <table class="w-full text-left text-[11px] border-collapse">
                                                                                                          <thead><tr class="bg-slate-50 text-slate-400 font-black uppercase tracking-wider text-[9px]">
                                                                                                            <th class="px-6 py-3">Date</th>
                                                                                                            <th class="px-6 py-3">Vaccine</th>
                                                                                                            <th class="px-6 py-3">Dose</th>
                                                                                                            <th class="px-6 py-3">Status</th>
                                                                                                            <!-- FIX: column header changed to match field name -->
                                                                                                            <th class="px-6 py-3">Given By</th>
                                                                                                            <th class="px-6 py-3">Lot #</th>
                                                                                                          </tr></thead>
                                                                                                          <tbody>
                                                                                                            <tr v-for="(rec, i) in filteredRecords" :key="i" class="border-t border-slate-50 hover:bg-[#fafdf8] transition-colors">
                                                                                                              <td class="px-6 py-4 whitespace-nowrap text-slate-500">
                                                                                                                {{ rec.dateAdministered
                                                                                                                    ? formatDisplayDate(new Date(rec.dateAdministered))
                                                                                                                    : rec.scheduledDate
                                                                                                                      ? formatDisplayDate(new Date(rec.scheduledDate)) + ' (scheduled)'
                                                                                                                      : '—' }}
                                                                                                              </td>
                                                                                                              <td class="px-6 py-4 font-bold text-slate-800">{{ rec.vaccineName }}</td>
                                                                                                              <td class="px-6 py-4 text-slate-500">Dose {{ rec.doseNumber }}</td>
                                                                                                              <td class="px-6 py-4"><span :class="getStatusClass(rec.status)" class="px-3 py-1 rounded-full text-[9px] font-black uppercase">{{ rec.status }}</span></td>
                                                                                                              <!-- FIX: was rec.administeredBy (GUID), now rec.administeredByName (doctor's name) -->
                                                                                                              <td class="px-6 py-4 text-slate-500 italic">{{ rec.administeredByName || '—' }}</td>
                                                                                                              <td class="px-6 py-4 font-mono text-[10px] text-slate-400">{{ rec.lotNumber || '—' }}</td>
                                                                                                            </tr>
                                                                                                          </tbody>
                                                                                                        </table>
                                                                                                      </div>
                                                                                                      <div class="px-8 py-5 bg-[#f8f9fa] border-t border-slate-100 flex justify-between items-center">
                                                                                                        <p class="text-[10px] text-slate-400 font-black uppercase">Full history available as PDF</p>
                                                                                                        <button class="bg-[#5d6b52] hover:bg-[#4a5741] text-white px-5 py-2 rounded-xl text-[9px] font-black uppercase tracking-widest transition-colors">Download PDF</button>
                                                                                                      </div>
                                                                                                    </div>
                                                                                                  </div>

                                                                                                </main>
                                                                                              </div>

                                                                                              <!-- PROFILE MODAL -->
                                                                                              <Transition name="modal-fade">
                                                                                                <div v-if="showProfile" class="fixed inset-0 z-300 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm" @click.self="showProfile = false">
                                                                                                  <div class="relative w-full max-w-2xl bg-white rounded-4xl shadow-2xl overflow-hidden max-h-[90vh] flex flex-col">
                                                                                                    <div class="bg-[#5d6b52] px-8 py-7 flex items-center gap-5 shrink-0">
                                                                                                      <div class="w-16 h-16 rounded-2xl bg-white/20 flex items-center justify-center text-4xl shadow-lg shrink-0">👤</div>
                                                                                                      <div class="flex-1 min-w-0">
                                                                                                        <p class="text-white font-black text-xl leading-tight">
                                                                                                          {{ parentData?.firstName }} {{ parentData?.middleName ? parentData.middleName + ' ' : '' }}{{ parentData?.lastName }}
                                                                                                        </p>
                                                                                                        <p class="text-[#c8d9b0] text-xs font-bold mt-1">Parent/Guardian Account · Aruga Pediatric Portal</p>
                                                                                                      </div>
                                                                                                      <button @click="showProfile = false" class="w-9 h-9 rounded-xl bg-white/10 hover:bg-white/25 flex items-center justify-center text-white text-lg transition-all shrink-0">✕</button>
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
                                                                                                      <button @click="showProfile = false" class="bg-[#5d6b52] hover:bg-[#4a5741] text-white px-6 py-2.5 rounded-xl text-[10px] font-black uppercase tracking-widest transition-colors shrink-0">Close</button>
                                                                                                    </div>
                                                                                                  </div>
                                                                                                </div>
                                                                                              </Transition>

                                                                                              <!-- NOTIFICATION PANEL -->
                                                                                              <Transition name="notif-panel">
                                                                                                <div v-if="showNotifications" class="fixed inset-0 z-200 flex justify-end" @click.self="showNotifications = false">
                                                                                                  <div class="absolute inset-0 bg-black/40 backdrop-blur-sm" @click="showNotifications = false"></div>
                                                                                                  <div class="relative w-full max-w-100 h-full bg-white shadow-2xl flex flex-col">
                                                                                                    <div class="bg-[#5d6b52] px-6 py-5 flex items-center justify-between text-white shrink-0">
                                                                                                      <div class="flex items-center gap-3">
                                                                                                        <span class="text-xl">🔔</span>
                                                                                                        <span class="font-black text-base">Notifications</span>
                                                                                                        <span v-if="unreadCount > 0" class="bg-[#ff4d4d] text-[10px] px-2.5 py-1 rounded-full font-black">{{ unreadCount }} NEW</span>
                                                                                                      </div>
                                                                                                      <div class="flex items-center gap-3">
                                                                                                        <button v-if="unreadCount > 0" @click="markAllRead" class="text-[10px] font-black text-white/70 hover:text-white uppercase tracking-wide transition-colors">Mark all read</button>
                                                                                                        <button @click="showNotifications = false" class="w-8 h-8 rounded-xl bg-white/10 hover:bg-white/20 flex items-center justify-center text-lg transition-colors">✕</button>
                                                                                                      </div>
                                                                                                    </div>
                                                                                                    <div class="flex gap-1 p-3 bg-[#f8f9f5] border-b border-slate-100 shrink-0">
                                                                                                      <button v-for="tab in notifTabs" :key="tab.key" @click="notifFilter = tab.key"
                                                                                                              :class="notifFilter === tab.key ? 'bg-[#5d6b52] text-white shadow-sm' : 'text-slate-500 hover:bg-white'"
                                                                                                              class="flex-1 py-1.5 rounded-xl text-[9px] font-black uppercase tracking-wide transition-all">
                                                                                                        {{ tab.label }}<span v-if="tab.count > 0" class="ml-1 opacity-70">({{ tab.count }})</span>
                                                                                                      </button>
                                                                                                    </div>
                                                                                                    <div class="flex-1 overflow-y-auto">
                                                                                                      <div v-if="notifsLoading" class="flex flex-col items-center justify-center h-40 text-slate-400"><p class="text-2xl mb-2 animate-pulse">🔔</p><p class="text-xs font-bold">Loading notifications...</p></div>
                                                                                                      <div v-else-if="filteredNotifications.length === 0" class="flex flex-col items-center justify-center h-40 text-slate-400 px-8 text-center"><p class="text-3xl mb-3">✅</p><p class="text-sm font-black text-slate-600">All caught up!</p><p class="text-xs text-slate-400 mt-1">No {{ notifFilter === 'all' ? '' : notifFilter + ' ' }}notifications.</p></div>
                                                                                                      <div v-else class="p-3 space-y-2">
                                                                                                        <div v-for="notif in filteredNotifications" :key="notif.notificationID" @click="markRead(notif)"
                                                                                                            :class="notif.isRead ? 'bg-white border-transparent' : 'bg-[#f8fdf5] border-[#99ad7a]/30'"
                                                                                                            class="p-4 rounded-2xl border cursor-pointer hover:shadow-sm transition-all">
                                                                                                          <div class="flex items-start gap-3">
                                                                                                            <div :class="notifIconBg(notif.type)" class="w-10 h-10 rounded-xl flex items-center justify-center text-lg shrink-0 shadow-sm">{{ notifIcon(notif.type) }}</div>
                                                                                                            <div class="flex-1 min-w-0">
                                                                                                              <div class="flex items-start justify-between gap-2">
                                                                                                                <p class="text-xs font-black text-slate-800 leading-tight">{{ notif.title }}</p>
                                                                                                                <span v-if="!notif.isRead" class="w-2 h-2 rounded-full bg-[#99ad7a] shrink-0 mt-1"></span>
                                                                                                              </div>
                                                                                                              <p class="text-[10px] text-slate-500 mt-1 leading-relaxed">{{ notif.message }}</p>
                                                                                                              <p class="text-[9px] text-slate-400 mt-2 font-bold uppercase tracking-wide">{{ formatRelativeTime(notif.createdAt) }}</p>
                                                                                                            </div>
                                                                                                          </div>
                                                                                                        </div>
                                                                                                      </div>
                                                                                                    </div>
                                                                                                    <div class="shrink-0 px-6 py-4 border-t border-slate-100 bg-[#f8f9f5]">
                                                                                                      <p class="text-[9px] text-slate-400 text-center font-bold uppercase tracking-wide">Reminders sent 1 month · 1 week · 5 days · 1 day before each vaccine</p>
                                                                                                    </div>
                                                                                                  </div>
                                                                                                </div>
                                                                                              </Transition>

                                                                                            </div>
                                                                                          </div>
                                                                                        </template>

                                                                                        <script setup>
                                                                                        import { ref, computed, onMounted, watch } from 'vue'
                                                                                        import { useRouter } from 'vue-router'
                                                                                        import axios from 'axios'
                                                                                        import { addDays, format, isMonday, isWednesday, isFriday, getDaysInMonth, startOfMonth } from 'date-fns'

                                                                                        const router = useRouter()
                                                                                        const API = 'http://localhost:57147'

                                                                                        // ── State ──────────────────────────────────────────────────────────────────
                                                                                        const parentData        = ref(null)
                                                                                        const children          = ref([])
                                                                                        const selectedChild     = ref(null)
                                                                                        const searchQuery       = ref('')
                                                                                        const activeNav         = ref('Overview')
                                                                                        const isProfileMenuOpen = ref(false)
                                                                                        const showNotifications = ref(false)
                                                                                        const showProfile       = ref(false)
                                                                                        const searchFocused     = ref(false)
                                                                                        const expandedChildren  = ref(new Set())

                                                                                        // Records
                                                                                        // completedRecords = raw Completed rows from DB
                                                                                        // vaccinationHistory = completedRecords + computed upcoming/overdue rows
                                                                                        const completedRecords   = ref([])
                                                                                        const vaccinationHistory = ref([])
                                                                                        const recordsLoading     = ref(false)
                                                                                        const recordFilter       = ref('All')
                                                                                        const recordStats        = ref({ completed: 0, scheduled: 0, overdue: 0 })

                                                                                        // Notifications
                                                                                        const notifications = ref([])
                                                                                        const notifsLoading = ref(false)
                                                                                        const notifFilter   = ref('all')
                                                                                        const unreadCount   = computed(() => notifications.value.filter(n => !n.isRead).length)

                                                                                        // ── Vaccine master (DOH schedule) ──────────────────────────────────────────
                                                                                        // gap = days from birth (dose 1) or days from previous dose (dose 2+)
                                                                                        const VACCINE_MASTER = [
                                                                                          { vaccineId: 1, name: 'BCG Vaccine',                      doses: [{ n: 1, gap: 0   }] },
                                                                                          { vaccineId: 2, name: 'Hepatitis B Vaccine',              doses: [{ n: 1, gap: 0   }] },
                                                                                          { vaccineId: 3, name: 'Pentavalent (DPT-Hep B-HIB)',      doses: [{ n: 1, gap: 45  }, { n: 2, gap: 28 }, { n: 3, gap: 28 }] },
                                                                                          { vaccineId: 4, name: 'Oral Polio Vaccine (OPV)',         doses: [{ n: 1, gap: 45  }, { n: 2, gap: 28 }, { n: 3, gap: 28 }] },
                                                                                          { vaccineId: 5, name: 'Inactivated Polio Vaccine (IPV)',  doses: [{ n: 1, gap: 105 }, { n: 2, gap: 165}] },
                                                                                          { vaccineId: 6, name: 'Pneumococcal Conj. Vaccine (PCV)', doses: [{ n: 1, gap: 45  }, { n: 2, gap: 28 }, { n: 3, gap: 28 }] },
                                                                                          { vaccineId: 7, name: 'MMR Vaccine',                      doses: [{ n: 1, gap: 270 }, { n: 2, gap: 90 }] },
                                                                                        ]

                                                                                        // ── Helpers ────────────────────────────────────────────────────────────────
                                                                                        function snapToClinicDay(date) {
                                                                                          let d = new Date(date)
                                                                                          while (!(isMonday(d) || isWednesday(d) || isFriday(d))) d = addDays(d, 1)
                                                                                          return d
                                                                                        }
                                                                                        function formatDisplayDate(date) {
                                                                                          if (!date) return '—'
                                                                                          try { return format(new Date(date), 'MMM d, yyyy') } catch { return '—' }
                                                                                        }
                                                                                        function formatRelativeTime(dateStr) {
                                                                                          if (!dateStr) return ''
                                                                                          const diff  = Date.now() - new Date(dateStr).getTime()
                                                                                          const mins  = Math.floor(diff / 60000)
                                                                                          const hours = Math.floor(diff / 3600000)
                                                                                          const days  = Math.floor(diff / 86400000)
                                                                                          if (mins < 1)   return 'Just now'
                                                                                          if (mins < 60)  return `${mins}m ago`
                                                                                          if (hours < 24) return `${hours}h ago`
                                                                                          if (days < 7)   return `${days}d ago`
                                                                                          return new Date(dateStr).toLocaleDateString('en-PH', { month: 'short', day: 'numeric' })
                                                                                        }
                                                                                        function notifIcon(type) {
                                                                                          const m = { ReminderMonth:'📅', ReminderWeek:'⏰', Reminder5Day:'⚡', ReminderDay:'🔔', OverdueMiss:'😟', Overdue5Day:'⚠️', Overdue2Week:'🚨', OverdueUrgent:'🆘', StockAlert:'📦', StockResolved:'✅' }
                                                                                          return m[type] ?? '🔔'
                                                                                        }
                                                                                        function notifIconBg(type) {
                                                                                          const m = { ReminderMonth:'bg-blue-100', ReminderWeek:'bg-amber-100', Reminder5Day:'bg-orange-100', ReminderDay:'bg-orange-200', OverdueMiss:'bg-yellow-100', Overdue5Day:'bg-orange-200', Overdue2Week:'bg-red-200', OverdueUrgent:'bg-red-400', StockAlert:'bg-red-100', StockResolved:'bg-green-100' }
                                                                                          return m[type] ?? 'bg-slate-100'
                                                                                        }
                                                                                        function getStatusClass(status) {
                                                                                          if (status === 'Completed') return 'bg-green-100 text-green-700'
                                                                                          if (status === 'Overdue')   return 'bg-red-100 text-red-700'
                                                                                          if (status === 'Scheduled') return 'bg-blue-100 text-blue-700'
                                                                                          return 'bg-yellow-100 text-yellow-700'
                                                                                        }
                                                                                        function toggleChildCard(childId) {
                                                                                          const next = new Set(expandedChildren.value)
                                                                                          next.has(childId) ? next.delete(childId) : next.add(childId)
                                                                                          expandedChildren.value = next
                                                                                        }

                                                                                        // ── Computed ───────────────────────────────────────────────────────────────
                                                                                        const searchResults = computed(() => {
                                                                                          const q = searchQuery.value.trim().toLowerCase()
                                                                                          if (!q) return []
                                                                                          return children.value.filter(c => `${c.firstName} ${c.lastName}`.toLowerCase().includes(q)).slice(0, 6)
                                                                                        })

                                                                                        const childAge = computed(() => {
                                                                                          if (!selectedChild.value?.birthDate) return '—'
                                                                                          const birth = new Date(selectedChild.value.birthDate)
                                                                                          const now   = new Date()
                                                                                          let years = now.getFullYear() - birth.getFullYear()
                                                                                          let months = now.getMonth() - birth.getMonth()
                                                                                          if (months < 0) { years--; months += 12 }
                                                                                          if (years === 0) return `${months} mo${months !== 1 ? 's' : ''}`
                                                                                          if (months === 0) return `${years} yr${years !== 1 ? 's' : ''}`
                                                                                          return `${years} yr${years !== 1 ? 's' : ''} ${months} mo${months !== 1 ? 's' : ''}`
                                                                                        })

                                                                                        // ── FIX: computedVaccineList now uses actual DateAdministered from DB
                                                                                        // for the 28-day DOH interval rule.
                                                                                        // If a dose was given late, the NEXT dose date is calculated from when
                                                                                        // it was actually given (not from the original schedule).
                                                                                        const computedVaccineList = computed(() => {
                                                                                          if (!selectedChild.value?.birthDate) return []
                                                                                          const birth = new Date(selectedChild.value.birthDate)
                                                                                          const today = new Date()
                                                                                          const result = []

                                                                                          for (const vaccine of VACCINE_MASTER) {
                                                                                            // Track the actual date the previous dose was given
                                                                                            // (for the 28-day cascade rule)
                                                                                            let prevActualDate   = null  // date previous dose was actually administered
                                                                                            let prevOriginalDate = null  // what the original schedule said for previous dose

                                                                                            for (const dose of vaccine.doses) {
                                                                                              // Look up whether this dose was actually completed in the DB
                                                                                              const record = completedRecords.value.find(r =>
                                                                                                Number(r.vaccineID ?? r.vaccineId) === vaccine.vaccineId &&
                                                                                                Number(r.doseNumber ?? r.DoseNumber) === dose.n &&
                                                                                                r.status === 'Completed' &&
                                                                                                r.dateAdministered
                                                                                              )

                                                                                              // Original scheduled date (ignoring late doses) — for "was late" display
                                                                                              const originalDueDate = dose.n === 1
                                                                                                ? snapToClinicDay(addDays(birth, dose.gap))
                                                                                                : snapToClinicDay(addDays(prevOriginalDate ?? birth, dose.gap))

                                                                                              // FIX: scheduledDate for pending doses uses actual previous administered date
                                                                                              // so if dose 1 was given 10 days late, dose 2 is scheduled 28 days after
                                                                                              // the actual administered date of dose 1 — not 28 days after the original schedule
                                                                                              let scheduledDate
                                                                                              if (record) {
                                                                                                scheduledDate = new Date(record.dateAdministered)
                                                                                              } else if (dose.n === 1) {
                                                                                                scheduledDate = snapToClinicDay(addDays(birth, dose.gap))
                                                                                              } else {
                                                                                                // cascade from actual previous dose date if available
                                                                                                const base = prevActualDate ?? prevOriginalDate ?? birth
                                                                                                scheduledDate = snapToClinicDay(addDays(base, dose.gap))
                                                                                              }

                                                                                              const administeredDate = record ? new Date(record.dateAdministered) : null
                                                                                              const wasLate = record ? administeredDate > originalDueDate : false
                                                                                              const daysLate = wasLate
                                                                                                ? Math.max(0, Math.round((administeredDate - originalDueDate) / 86400000))
                                                                                                : 0

                                                                                              prevOriginalDate = originalDueDate
                                                                                              prevActualDate   = administeredDate ?? null

                                                                                              result.push({
                                                                                                doseId:          `${vaccine.vaccineId}-${dose.n}`,
                                                                                                vaccineId:       vaccine.vaccineId,
                                                                                                name:            vaccine.name,
                                                                                                doseNumber:      dose.n,
                                                                                                scheduledDate,
                                                                                                originalDueDate, // the unaffected original schedule date (for "X days late" display)
                                                                                                isCompleted:     !!record,
                                                                                                administeredDate,
                                                                                                wasLate,
                                                                                                daysLate,
                                                                                              })
                                                                                            }
                                                                                          }
                                                                                          return result
                                                                                        })

                                                                                        const groupedVaccineList = computed(() => {
                                                                                          const map = new Map()
                                                                                          for (const vax of computedVaccineList.value) {
                                                                                            if (!map.has(vax.name)) map.set(vax.name, { name: vax.name, doses: [] })
                                                                                            map.get(vax.name).doses.push(vax)
                                                                                          }
                                                                                          return Array.from(map.values())
                                                                                        })

                                                                                        // Upcoming = pending doses only, sorted by scheduled date
                                                                                        const upcomingDoses = computed(() =>
                                                                                          computedVaccineList.value.filter(v => !v.isCompleted).sort((a, b) => a.scheduledDate - b.scheduledDate)
                                                                                        )

                                                                                        // FIX: filteredRecords uses correct status strings matching what vaccinationHistory actually contains
                                                                                        const filteredRecords = computed(() => {
                                                                                          if (recordFilter.value === 'All') return vaccinationHistory.value
                                                                                          return vaccinationHistory.value.filter(r => r.status === recordFilter.value)
                                                                                        })

                                                                                        const notifTabs = computed(() => [
                                                                                          { key: 'all',      label: 'All',      count: notifications.value.length },
                                                                                          { key: 'reminder', label: 'Upcoming', count: notifications.value.filter(n => n.type?.startsWith('Reminder')).length },
                                                                                          { key: 'overdue',  label: 'Overdue',  count: notifications.value.filter(n => n.type?.startsWith('Overdue')).length },
                                                                                          { key: 'stock',    label: 'Stock',    count: notifications.value.filter(n => n.type?.startsWith('Stock')).length },
                                                                                          { key: 'unread',   label: 'Unread',   count: unreadCount.value },
                                                                                        ])
                                                                                        const filteredNotifications = computed(() => {
                                                                                          switch (notifFilter.value) {
                                                                                            case 'reminder': return notifications.value.filter(n => n.type?.startsWith('Reminder'))
                                                                                            case 'overdue':  return notifications.value.filter(n => n.type?.startsWith('Overdue'))
                                                                                            case 'stock':    return notifications.value.filter(n => n.type?.startsWith('Stock'))
                                                                                            case 'unread':   return notifications.value.filter(n => !n.isRead)
                                                                                            default:
                                                                                              return [...notifications.value].sort((a, b) => {
                                                                                                if (!a.isRead && b.isRead) return -1
                                                                                                if (a.isRead && !b.isRead) return 1
                                                                                                return new Date(b.createdAt) - new Date(a.createdAt)
                                                                                              })
                                                                                          }
                                                                                        })

                                                                                        // ── Calendar ───────────────────────────────────────────────────────────────
                                                                                        const selectedVax = ref(null)
                                                                                        const calYear     = ref(new Date().getFullYear())
                                                                                        const calMonth    = ref(new Date().getMonth())

                                                                                        const calendarMonthLabel  = computed(() => format(new Date(calYear.value, calMonth.value, 1), 'MMMM yyyy'))
                                                                                        const calendarDaysInMonth = computed(() => getDaysInMonth(new Date(calYear.value, calMonth.value, 1)))
                                                                                        const calendarOffset      = computed(() => startOfMonth(new Date(calYear.value, calMonth.value, 1)).getDay())
                                                                                        const windowEndDate       = computed(() => selectedVax.value ? addDays(selectedVax.value.scheduledDate, 14) : null)

                                                                                        function prevMonth() { calMonth.value === 0 ? (calMonth.value = 11, calYear.value--) : calMonth.value-- }
                                                                                        function nextMonth() { calMonth.value === 11 ? (calMonth.value = 0, calYear.value++) : calMonth.value++ }

                                                                                        function getDayClass(day) {
                                                                                          if (!selectedVax.value) return 'text-slate-200'
                                                                                          const date      = new Date(calYear.value, calMonth.value, day)
                                                                                          const scheduled = selectedVax.value.scheduledDate
                                                                                          const winEnd    = windowEndDate.value
                                                                                          const clinic    = isMonday(date) || isWednesday(date) || isFriday(date)
                                                                                          const sameDay   = (a, b) => a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate()

                                                                                          if (selectedVax.value.wasLate && selectedVax.value.originalDueDate && sameDay(date, selectedVax.value.originalDueDate))
                                                                                            return 'bg-red-400 text-white shadow-lg scale-110 font-black z-10 cursor-default'
                                                                                          if (selectedVax.value.isCompleted && selectedVax.value.administeredDate && sameDay(date, selectedVax.value.administeredDate))
                                                                                            return 'bg-green-500 text-white shadow-lg scale-110 font-black z-10 cursor-default'
                                                                                          if (!selectedVax.value.isCompleted && sameDay(date, scheduled))
                                                                                            return 'bg-[#5d6b52] text-white shadow-lg scale-110 font-black z-10 cursor-default'
                                                                                          if (!selectedVax.value.isCompleted && clinic && winEnd && date > scheduled && date <= winEnd)
                                                                                            return 'bg-[#99ad7a]/30 text-[#3a4a2e] border-b-2 border-[#99ad7a] cursor-pointer'
                                                                                          if (clinic) return 'bg-slate-100 text-slate-400 cursor-pointer'
                                                                                          return 'text-slate-300 pointer-events-none'
                                                                                        }

                                                                                        // ── Watchers ───────────────────────────────────────────────────────────────
                                                                                        watch(computedVaccineList, (list) => {
                                                                                          if (list.length && (!selectedVax.value || !list.find(v => v.doseId === selectedVax.value?.doseId)))
                                                                                            selectedVax.value = list[0]
                                                                                        }, { immediate: true })

                                                                                        watch(selectedVax, (vax) => {
                                                                                          if (vax?.scheduledDate) {
                                                                                            calYear.value  = new Date(vax.scheduledDate).getFullYear()
                                                                                            calMonth.value = new Date(vax.scheduledDate).getMonth()
                                                                                          }
                                                                                        }, { immediate: true })

                                                                                        // ── Actions ────────────────────────────────────────────────────────────────
                                                                                        function selectFromSearch(child) {
                                                                                          selectChild(child)
                                                                                          searchQuery.value  = ''
                                                                                          searchFocused.value = false
                                                                                          activeNav.value    = 'Overview'
                                                                                        }
                                                                                        function onSearchBlur() { setTimeout(() => { searchFocused.value = false }, 150) }
                                                                                        function toggleProfileMenu() { isProfileMenuOpen.value = !isProfileMenuOpen.value }
                                                                                        function handleLogout() { localStorage.removeItem('parentUser'); router.push('/Login') }

                                                                                        // FIX: selectChild is now a proper function so sidebar clicks also re-fetch records
                                                                                        function selectChild(child) {
                                                                                          selectedChild.value = child
                                                                                          fetchRecords(child.childID)
                                                                                        }

                                                                                        async function markRead(notif) {
                                                                                          if (notif.isRead) return
                                                                                          notif.isRead = true
                                                                                          try { await axios.patch(`${API}/api/Notifications/mark-read/${notif.notificationID}`) } catch {}
                                                                                        }
                                                                                        async function markAllRead() {
                                                                                          if (!parentData.value?.parentID) return
                                                                                          notifications.value.forEach(n => { n.isRead = true })
                                                                                          try { await axios.patch(`${API}/api/Notifications/mark-all-read/${parentData.value.parentID}`) } catch {}
                                                                                        }

                                                                                        // FIX: fetchRecords now stores raw completed records in completedRecords ref
                                                                                        // THEN builds the combined vaccinationHistory (completed + computed upcoming)
                                                                                        // AFTER completedRecords is set — no more circular dependency
                                                                                        async function fetchRecords(childId) {
                                                                                          if (!childId) return
                                                                                          recordsLoading.value = true
                                                                                          try {
                                                                                            const res = await axios.get(`${API}/api/VaccinationRecords/child/${childId}`)
                                                                                            // Store only DB records here — computedVaccineList reads from this
                                                                                            completedRecords.value = res.data

                                                                                            // Wait one tick so computedVaccineList updates with fresh completedRecords
                                                                                            await new Promise(r => setTimeout(r, 0))

                                                                                            // Build full history: DB completed rows + computed upcoming/overdue rows
                                                                                            const today = new Date()
                                                                                            const upcomingRows = computedVaccineList.value
                                                                                              .filter(v => !v.isCompleted)
                                                                                              .map(v => ({
                                                                                                vaccineID:         v.vaccineId,
                                                                                                vaccineName:       v.name,
                                                                                                doseNumber:        v.doseNumber,
                                                                                                dateAdministered:  null,
                                                                                                scheduledDate:     v.scheduledDate,
                                                                                                // FIX: status matches filter button values exactly
                                                                                                status:            v.scheduledDate < today ? 'Overdue' : 'Scheduled',
                                                                                                administeredByName: null,
                                                                                                lotNumber:         null,
                                                                                              }))

                                                                                            vaccinationHistory.value = [...res.data, ...upcomingRows].sort((a, b) => {
                                                                                              const dA = a.dateAdministered ? new Date(a.dateAdministered) : new Date(a.scheduledDate)
                                                                                              const dB = b.dateAdministered ? new Date(b.dateAdministered) : new Date(b.scheduledDate)
                                                                                              return dA - dB
                                                                                            })

                                                                                            recordStats.value = {
                                                                                              completed: res.data.length,
                                                                                              scheduled: upcomingRows.filter(r => r.status === 'Scheduled').length,
                                                                                              overdue:   upcomingRows.filter(r => r.status === 'Overdue').length,
                                                                                            }
                                                                                          } catch (err) {
                                                                                            console.error('fetchRecords error:', err)
                                                                                            completedRecords.value   = []
                                                                                            vaccinationHistory.value = []
                                                                                            recordStats.value = { completed: 0, scheduled: 0, overdue: 0 }
                                                                                          } finally {
                                                                                            recordsLoading.value = false
                                                                                          }
                                                                                        }

                                                                                        async function fetchNotifications() {
                                                                                          if (!parentData.value?.parentID) return
                                                                                          notifsLoading.value = true
                                                                                          try {
                                                                                            const res = await axios.get(`${API}/api/Notifications/parent/${parentData.value.parentID}`)
                                                                                            notifications.value = res.data
                                                                                          } catch {
                                                                                            notifications.value = []
                                                                                          } finally {
                                                                                            notifsLoading.value = false
                                                                                          }
                                                                                        }

                                                                                        // ── Lifecycle ──────────────────────────────────────────────────────────────
                                                                                        onMounted(async () => {
                                                                                          const savedUser = localStorage.getItem('parentUser')
                                                                                        // if (!savedUser) { router.push('/'); return }

                                                                                          parentData.value = JSON.parse(savedUser)

                                                                                        try {
                                                                                          const res = await axios.get(
                                                                                            `${API}/api/Parents/${parentData.value.parentID}/dashboard`
                                                                                          )

                                                                                          children.value = res.data.map(child => ({
                                                                                            childID: child.ChildID ?? child.childID,
                                                                                            firstName: child.FirstName ?? child.firstName,
                                                                                            middleName: child.MiddleName ?? child.middleName,
                                                                                            lastName: child.LastName ?? child.lastName,
                                                                                            birthDate: child.BirthDate ?? child.birthDate,
                                                                                            placeOfBirth: child.PlaceOfBirth ?? child.placeOfBirth,
                                                                                            sex: child.Sex ?? child.sex,
                                                                                            barangay: child.Barangay ?? child.barangay,
                                                                                            address: child.Address ?? child.address,
                                                                                            healthCenter: child.HealthCenter ?? child.healthCenter,

                                                                                            // Relationship information
                                                                                            relationshipType: child.RelationshipType ?? child.relationshipType,
                                                                                            isPrimaryContact: child.IsPrimaryContact ?? child.isPrimaryContact,
                                                                                            canReceiveNotifications:
                                                                                              child.CanReceiveNotifications ?? child.canReceiveNotifications,

                                                                                            // Keep these for your existing profile UI
                                                                                            motherName: child.MotherName ?? child.motherName ?? null,
                                                                                            fatherName: child.FatherName ?? child.fatherName ?? null,
                                                                                            guardianName: child.GuardianName ?? child.guardianName ?? null,
                                                                                          }))

                                                                                        } catch (err) {
                                                                                          console.error('Error fetching parent dashboard:', err)
                                                                                          children.value = []
                                                                                        }

                                                                                          if (children.value.length > 0) {
                                                                                            selectedChild.value = children.value[0]
                                                                                            await fetchRecords(selectedChild.value.childID)
                                                                                          }

                                                                                          await fetchNotifications()
                                                                                        })

                                                                                        // ── Change Password ────────────────────────────────────────────
                                                                                        const showChangePw  = ref(false)
                                                                                        const pwForm        = ref({ current: '', newPw: '', confirm: '' })
                                                                                        const pwError       = ref('')
                                                                                        const pwSuccess     = ref('')
                                                                                        const pwLoading     = ref(false)

                                                                                        async function changePassword() {
                                                                                          pwError.value   = ''
                                                                                          pwSuccess.value = ''
                                                                                          console.log('parentData:', JSON.stringify(parentData.value))
                                                                                          console.log('Sending to:', `${API}/api/Parents/${parentData.value.parentID}/change-password`)
                                                                                          if (!pwForm.value.current)          { pwError.value = 'Please enter your current password.'; return }
                                                                                          if (pwForm.value.newPw.length < 6)  { pwError.value = 'New password must be at least 6 characters.'; return }
                                                                                          if (pwForm.value.newPw !== pwForm.value.confirm) { pwError.value = 'Passwords do not match.'; return }

                                                                                          pwLoading.value = true
                                                                                          try {
                                                                                            await axios.patch(`${API}/api/Parents/${parentData.value.parentID}/change-password`, {
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

                                                                                        .notif-panel-enter-active, .notif-panel-leave-active { transition: opacity 0.25s ease; }
                                                                                        .notif-panel-enter-active > div:last-child, .notif-panel-leave-active > div:last-child { transition: transform 0.3s cubic-bezier(0.4,0,0.2,1); }
                                                                                        .notif-panel-enter-from, .notif-panel-leave-to { opacity: 0; }
                                                                                        .notif-panel-enter-from > div:last-child, .notif-panel-leave-to > div:last-child { transform: translateX(100%); }

                                                                                        @keyframes scanline {
                                                                                          0%, 100% { top: 8px; opacity: 1; }
                                                                                          50% { top: calc(100% - 8px); opacity: 0.6; }
                                                                                        }
                                                                                        </style>