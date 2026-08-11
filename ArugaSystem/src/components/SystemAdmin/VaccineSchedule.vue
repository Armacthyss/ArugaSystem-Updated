<template>
  <div class="min-h-screen bg-gray-50 p-6">

    <!-- Header -->
    <div class="mb-6">
      <h1 class="text-3xl font-bold text-gray-800">
        Vaccine Schedule
      </h1>

      <p class="mt-1 text-gray-500">
        Vaccination schedule based on the configured vaccine rules.
      </p>
    </div>

    <!-- Loading -->
    <div
      v-if="loading"
      class="rounded-xl bg-white p-8 text-center shadow"
    >
      <p class="text-gray-500">
        Loading vaccine schedule...
      </p>
    </div>

    <!-- Error -->
    <div
      v-else-if="error"
      class="rounded-xl border border-red-200 bg-red-50 p-5"
    >
      <p class="font-semibold text-red-700">
        Unable to load vaccine schedule
      </p>

      <p class="mt-1 text-sm text-red-600">
        {{ error }}
      </p>

      <button
        @click="loadSchedule"
        class="mt-4 rounded-lg bg-red-600 px-4 py-2 text-white hover:bg-red-700"
      >
        Retry
      </button>
    </div>

    <!-- Schedule -->
    <div
      v-else
      class="overflow-hidden rounded-xl bg-white shadow"
    >

      <!-- Table -->
      <div class="overflow-x-auto">

        <table class="min-w-max w-full border-collapse">

          <!-- Header -->
          <thead>
            <tr>
              <th
                class="sticky left-0 z-20 min-w-[240px] border border-gray-200 bg-orange-400 px-5 py-4 text-left font-bold text-gray-900"
              >
                Vaccine
              </th>

              <th
                class="min-w-[220px] border border-gray-200 bg-orange-400 px-5 py-4 text-left font-bold text-gray-900"
              >
                Disease
              </th>

              <!-- Dynamic visits -->
              <th
                v-for="visit in visits"
                :key="visit.sequence"
                class="min-w-[150px] border border-gray-200 bg-orange-400 px-4 py-3 text-center font-bold text-gray-900"
              >
                {{ visit.label }}
              </th>
            </tr>
          </thead>

          <!-- Body -->
          <tbody>

            <tr
              v-for="vaccine in scheduleRows"
              :key="vaccine.vaccineID"
              class="hover:bg-gray-50"
            >

              <!-- Vaccine -->
              <td
                class="sticky left-0 z-10 border border-gray-200 bg-white px-5 py-4 font-semibold text-gray-800"
              >
                {{ vaccine.vaccineName }}
              </td>

              <!-- Disease -->
              <td
                class="border border-gray-200 px-5 py-4 text-gray-700"
              >
                {{ vaccine.targetDisease || '—' }}
              </td>

              <!-- Dynamic visit cells -->
              <td
                v-for="visit in visits"
                :key="`${vaccine.vaccineID}-${visit.sequence}`"
                class="border border-gray-200 px-4 py-4 text-center"
              >

                <template
                  v-if="getRulesForVisit(vaccine.vaccineID, visit.sequence).length"
                >

                  <div class="flex flex-col items-center gap-1">

                    <!-- Check -->
                    <div
                      class="flex h-9 w-9 items-center justify-center rounded-full bg-orange-400 text-lg font-bold text-gray-800"
                    >
                      ✓
                    </div>

                    <!-- Dose information -->
                    <div
                      v-for="rule in getRulesForVisit(
                        vaccine.vaccineID,
                        visit.sequence
                      )"
                      :key="rule.ruleID"
                      class="text-xs font-medium text-gray-600"
                    >
                      Dose {{ rule.doseNumber }}
                    </div>

                  </div>

                </template>

                <span
                  v-else
                  class="text-gray-300"
                >
                  —
                </span>

              </td>

            </tr>

            <!-- Empty -->
            <tr v-if="scheduleRows.length === 0">
              <td
                :colspan="2 + visits.length"
                class="px-6 py-12 text-center text-gray-500"
              >
                No vaccine schedule rules have been configured yet.
              </td>
            </tr>

          </tbody>

        </table>

      </div>

      <!-- Footer -->
      <div
        class="border-t border-gray-200 bg-gray-50 px-5 py-4 text-sm text-gray-500"
      >
        Schedule is generated from the currently configured vaccination
        schedule rules.
      </div>

    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import axios from 'axios'

const API_BASE = 'http://localhost:57147/api'

const vaccines = ref([])
const rules = ref([])

const loading = ref(true)
const error = ref(null)

/*
|--------------------------------------------------------------------------
| Load data
|--------------------------------------------------------------------------
*/

const loadSchedule = async () => {
  loading.value = true
  error.value = null

  try {
    const [vaccinesResponse, rulesResponse] = await Promise.all([
      axios.get(`${API_BASE}/Vaccines`),
      axios.get(`${API_BASE}/VaccinationScheduleRules`)
    ])

    vaccines.value = vaccinesResponse.data || []
    rules.value = rulesResponse.data || []

  } catch (err) {

    console.error('Failed to load vaccine schedule:', err)

    error.value =
      err.response?.data?.message ||
      err.message ||
      'Failed to load vaccine schedule.'

  } finally {
    loading.value = false
  }
}

/*
|--------------------------------------------------------------------------
| Format recommended age
|--------------------------------------------------------------------------
|
| The database stores the actual value as days.
| The frontend converts it into a readable label.
|
*/

const formatAge = (days) => {

  if (days === 0) {
    return 'At Birth'
  }

  if (days % 7 === 0 && days < 365) {
    const weeks = days / 7

    return `${weeks} Week${weeks === 1 ? '' : 's'}`
  }

  if (days >= 365) {

    const years = Math.floor(days / 365)
    const remainingDays = days % 365

    if (remainingDays === 0) {
      return `${years} Year${years === 1 ? '' : 's'}`
    }

    const months = Math.round(days / 30.4375)

    return `${months} Month${months === 1 ? '' : 's'}`
  }

  const months = Math.round(days / 30.4375)

  if (months >= 1) {
    return `${months} Month${months === 1 ? '' : 's'}`
  }

  return `${days} Day${days === 1 ? '' : 's'}`
}

/*
|--------------------------------------------------------------------------
| Dynamic visit columns
|--------------------------------------------------------------------------
|
| Nothing here is hardcoded.
|
| SequenceOrder determines the order of the columns.
| RecommendedAgeDays determines the displayed age.
|
*/

const visits = computed(() => {

  const visitMap = new Map()

  rules.value.forEach(rule => {

    const sequence = rule.sequenceOrder

    if (!visitMap.has(sequence)) {

      visitMap.set(sequence, {
        sequence,
        recommendedAgeDays: rule.recommendedAgeDays,
        label: formatAge(rule.recommendedAgeDays)
      })

    }

  })

  return Array.from(visitMap.values())
    .sort((a, b) => a.sequence - b.sequence)
})

/*
|--------------------------------------------------------------------------
| Combine vaccines + rules
|--------------------------------------------------------------------------
*/

const scheduleRows = computed(() => {

  return vaccines.value
    .filter(vaccine => vaccine.status !== false)
    .map(vaccine => {

      return {
        vaccineID: vaccine.vaccineID,
        vaccineName: vaccine.vaccineName,
        targetDisease: vaccine.targetDisease,

        rules: rules.value.filter(
          rule => rule.vaccineID === vaccine.vaccineID
        )
      }

    })
})

/*
|--------------------------------------------------------------------------
| Find rules for a vaccine + visit
|--------------------------------------------------------------------------
*/

const getRulesForVisit = (vaccineID, sequence) => {

  return rules.value.filter(rule =>
    rule.vaccineID === vaccineID &&
    rule.sequenceOrder === sequence
  )

}

onMounted(loadSchedule)
</script>