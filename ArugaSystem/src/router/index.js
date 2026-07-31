import { createRouter, createWebHistory } from 'vue-router';

// ── Existing imports (unchanged) ─────────────────────────────

import Login        from '@/components/Login/Login.vue';
import StaffLogin   from '@/components/Login/StaffLogin.vue';
import Admin        from '@/components/Login/Admin.vue';
import ParentHome   from '@/components/ParentViews/ParentHomepage.vue';
import AdminHomepage from '@/components/Admin/AdminHomepage.vue';

// ── Admin sub-pages (add these — match your actual filenames) ─
import StaffCalendar         from '@/components/Admin/StaffCalendar.vue';
import StaffChildRecord      from '@/components/Admin/StaffChildRecord.vue';
import StaffDoctorNurse      from '@/components/Admin/StaffDoctorNurse.vue';
import StaffParentRecord     from '@/components/Admin/StaffParentRecord.vue';
import StaffReport           from '@/components/Admin/StaffReport.vue';
import StaffVaccineInventory from '@/components/Admin/StaffVaccineInventory.vue';

// ── Doctor / Nurse pages (new) ────────────────────────────────
import DoctorHomepage from '@/components/Doctor/Nurse/DoctorHomepage.vue';
import DoctorPatients from '@/components/Doctor/Nurse/DoctorPatients.vue';
import DoctorCalendar from '@/components/Doctor/Nurse/DoctorCalendar.vue';
import DoctorVaccinationRecords from '@/components/Doctor/Nurse/DoctorVaccinationRecords.vue';
import DoctorReports  from '@/components/Doctor/Nurse/DoctorReports.vue';
import DoctorAccount  from '@/components/Doctor/Nurse/DoctorAccount.vue';

// ── SystemAdmin ─────────────────────────────────────────────
import SystemAdminHomepage from '@/components/SystemAdmin/SystemAdmin.vue';
import SystemAdminUserManagement from '@/components/SystemAdmin/SysAd-UserManagement.vue';
import SystemAdminPatient from '@/components/SystemAdmin/SysAd-Patient.vue';
import SystemAdminVaccine from '@/components/SystemAdmin/SysAd-Vaccine.vue';
import SystemAdminInventory from '@/components/SystemAdmin/SysAd-Inventory.vue';
import SystemAdminNotification from '@/components/SystemAdmin/SysAd-Notification.vue';
import SystemAdminReports from '@/components/SystemAdmin/SysAd-Reports.vue';
import SystemAuditlogs from '@/components/SystemAdmin/SysAd-Auditlogs.vue';
import TestAPI from '@/components/SystemAdmin/TestAPI.vue';

// ── Staff (Second version) ──────────────────────────────────
import StaffDashboard from '@/components/Staff/StaffDashboard.vue';
import StaffPatientRecords from '@/components/Staff/StaffPatientRecords.vue';
import StaffVaccineSchedule from '@/components/Staff/StaffVaccineSchedule.vue';

const routes = [
  // ── Old routes (keep exactly as they were) ─────────────────
 { path: '/',       component: Login        },
  { path: '/StaffLogin',  component: StaffLogin   },
  { path: '/admin-login', component: Admin        },
  { path: '/ParentHome',  component: ParentHome   },
  { path: '/AdminHome',   component: AdminHomepage },

  // ── Doctor / Nurse (new) ───────────────────────────────────
  { path: '/doctor/home',     name: 'DoctorHome',     component: DoctorHomepage },
  { path: '/doctor/patients', name: 'DoctorPatients', component: DoctorPatients },
  { path: '/doctor/calendar', name: 'DoctorCalendar', component: DoctorCalendar },
  { path: '/doctor/records', name: 'DoctorVaccinationRecords', component: DoctorVaccinationRecords },
  { path: '/doctor/reports',  name: 'DoctorReports',  component: DoctorReports  },
  { path: '/doctor/account',  name: 'DoctorAccount',  component: DoctorAccount  },

  // ── Admin sub-pages (new) ──────────────────────────────────
  { path: '/admin/calendar',          component: StaffCalendar         },
  { path: '/admin/children',          component: StaffChildRecord      },
  { path: '/admin/staff',             component: StaffDoctorNurse      },
  { path: '/admin/parents',           component: StaffParentRecord     },
  { path: '/admin/reports',           component: StaffReport           },
  { path: '/admin/vaccine-inventory', component: StaffVaccineInventory },

  {path: '/system-admin/home', component: SystemAdminHomepage},
  {path: '/system-admin/user-management', component: SystemAdminUserManagement},
  {path: '/system-admin/patients', component: SystemAdminPatient},
  {path: '/system-admin/vaccines', component: SystemAdminVaccine},
  {path: '/system-admin/inventory', component: SystemAdminInventory},
  {path: '/system-admin/notifications', component: SystemAdminNotification},
  {path: '/system-admin/reports', component: SystemAdminReports},
  {path: '/system-admin/audit-logs', component: SystemAuditlogs},
  {path: '/system-admin/test-api', component: TestAPI},

  // ── Staff (Second version) ──────────────────────────────────
  {path: '/staff/dashboard', component: StaffDashboard},
  {path: '/staff/patient-records', component: StaffPatientRecords},
  {path: '/staff/vaccine-schedule', component: StaffVaccineSchedule},
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router