<script setup>
onMounted(() => {
    console.log("Mounted!");
    loadAll();
});
import { ref, computed, onMounted, h } from "vue";
import {
  Home, Users, Syringe, Package, Bell, BarChart2, Settings, LogOut,
  ClipboardList, ChevronLeft, ChevronRight, Search,
  UserPlus, Eye, Pencil, Link2, KeyRound, Ban, Archive as ArchiveIcon,
  MoreHorizontal, X, MapPin, Phone, Mail, Baby, Check,
  ArrowRightLeft, UserCircle2, Star, ShieldCheck, Trash2,
} from "lucide-vue-next";
import axios from "axios";

/* =========================================================================
   API LAYER — talks directly to ParentsController / ChildrenController.
   (No api.js service file was provided, so this wires axios straight to the
   endpoints that exist in the two controllers you shared. If you already
   have a shared axios instance with baseURL / auth headers configured, 
   swap this `axios` import for that instance.)
========================================================================= */
const API_BASE = "http://localhost:57147/api";
const api = {
  getAllParents:                ()            => axios.get(`${API_BASE}/Parents/all`).then(r => r.data),
  createParent:                 (payload)     => axios.post(`${API_BASE}/Parents`, payload).then(r => r.data),
  updateParent:                 (id, payload) => axios.put(`${API_BASE}/Parents/${id}`, payload).then(r => r.data),
  getAllChildren:               ()            => axios.get(`${API_BASE}/Children/all`).then(r => r.data),
  getChildrenByParent:          (parentId)    => axios.get(`${API_BASE}/Children/parent/${parentId}`).then(r => r.data),
  createChild:                  (payload)     => axios.post(`${API_BASE}/Children`, payload).then(r => r.data),
  updateChild:                  (id, payload) => axios.put(`${API_BASE}/Children/${id}`, payload).then(r => r.data),
  getChildVaccinations:         (childId)     => axios.get(`${API_BASE}/VaccinationRecords/child/${childId}`).then(r => r.data),
  saveHistoricalVaccinations:   (payload)     => axios.post(`${API_BASE}/VaccinationRecords/historical`, payload).then(r => r.data),

  // Real Child <-> Parent/Guardian relationship API
  getRelationshipsByChild:      (childId)     => axios.get(`${API_BASE}/ChildParentRelationships/child/${childId}`).then(r => r.data),
  getRelationshipsByParent:     (parentId)    => axios.get(`${API_BASE}/ChildParentRelationships/parent/${parentId}`).then(r => r.data),
  createRelationship:           (payload)     => axios.post(`${API_BASE}/ChildParentRelationships`, payload).then(r => r.data),
  deleteRelationship:           (relationshipId) => axios.delete(`${API_BASE}/ChildParentRelationships/${relationshipId}`).then(r => r.data),
};

/* ---------- tiny render-fn components (badge / avatar markup, used everywhere) ---------- */
const statusStyle = { Active:"bg-emerald-50 text-emerald-700", Inactive:"bg-stone-100 text-stone-600", Pending:"bg-amber-50 text-amber-700", Archived:"bg-rose-50 text-rose-700" };
const statusDot   = { Active:"bg-emerald-600", Inactive:"bg-stone-400", Pending:"bg-amber-500", Archived:"bg-rose-600" };
const vaccStyle   = { "Fully Vaccinated":"bg-emerald-50 text-emerald-700", "Partially Vaccinated":"bg-amber-50 text-amber-700", Delayed:"bg-rose-50 text-rose-700", Upcoming:"bg-sky-50 text-sky-700" };
const avatarSize  = { sm:"h-9 w-9 text-[11px]", md:"h-10 w-10 text-[11px]", lg:"h-14 w-14 text-[15px]" };

const StatusBadge = (p) => h("span", { class:`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ${statusStyle[p.status]}` }, [h("span",{class:`h-1.5 w-1.5 rounded-full ${statusDot[p.status]}`}), p.status]);
const VaccBadge   = (p) => h("span", { class:`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium ${vaccStyle[p.status]}` }, p.status);
const Avatar      = (p) => h("div", { class:`flex items-center justify-center rounded-full text-white font-semibold shrink-0 bg-sky-700 ${avatarSize[p.size||"sm"]}` }, p.text);

const collapsed = ref(false);
const isLoading = ref(false);
const error = ref(null);
const activeTab = ref("parents"); // "parents" | "children"

const navItems = [
  { icon:Home, label:"Dashboard" }, 
  { icon:Users, label:"Patient Management", active:true },
  { icon:Syringe, label:"Vaccine Schedule" }, { icon:Package, label:"Inventory" },
  { icon:ClipboardList, label:"Queue Management" }, { icon:Bell, label:"Notifications" },
  { icon:BarChart2, label:"Clinic Reports" }, { icon:Settings, label:"Settings" },
];

const relationshipOptions = ["Mother","Father","Guardian","Grandparent","Foster Parent","Other Authorized Guardian"];
const linkRelationshipOptions = ["Mother","Father","Guardian","Grandmother","Grandfather","Aunt","Uncle","Sibling","Foster Parent","Relative","Other"];

// NOTE: no vaccine list endpoint/service exists elsewhere in this file, so the known
// vaccine catalog is defined here. If a shared vaccine service/dropdown is added later,
// swap this constant out for that service's data instead of maintaining two lists.
const VACCINE_OPTIONS = [
  { id:1,  name:"BCG Vaccine" },
  { id:2,  name:"Hepatitis B Vaccine" },
  { id:3,  name:"Pentavalent Vaccine (DPT-Hep B-HIB)" },
  { id:4,  name:"Oral Polio Vaccine (OPV)" },
  { id:5,  name:"Inactivated Polio Vaccine (IPV)" },
  { id:6,  name:"Pneumococcal Conjugate Vaccine (PCV)" },
  { id:7,  name:"Measles, Mumps, Rubella Vaccine (MMR)" },
  { id:9,  name:"BCG" },
  { id:10, name:"Covid" },
  { id:11, name:"Pneumoccal Conjugate Vaccine" },
];
const vaccineNameById = (id) => VACCINE_OPTIONS.find(v => v.id === Number(id))?.name || `Vaccine #${id}`;

/* ============================= Core data (loaded from API) ============================= */
const parents = ref([]);
const children = ref([]);

function calculateAge(birth) {
  const today = new Date();
  let age = today.getFullYear() - birth.getFullYear();
  const m = (today.getMonth() - birth.getMonth() + 12) % 12;
  if (today.getMonth() - birth.getMonth() < 0 || (today.getMonth()===birth.getMonth() && today.getDate()<birth.getDate())) age--;
  return `${age}y ${m}m`;
}
function toDateInputValue(iso) {
  if (!iso) return "";
  const d = new Date(iso);
  return isNaN(d) ? String(iso).slice(0,10) : d.toISOString().slice(0,10);
}

const mapParent = (p) => ({
  id:p.parentID, initials:`${p.firstName[0]}${p.lastName[0]}`.toUpperCase(), name:`${p.firstName} ${p.lastName}`,
  relationship:"Parent", contact:p.contactNo, email:p.email, username:p.email.split("@")[0],
  // NOTE: ParentsController has no Status field/endpoint — hardcoded until the API exposes one.
  status:"Active", address:p.address, barangay:p.barangayNo,
  raw:p, // original API record, kept so edits that don't touch every field don't lose data
});
const mapChild = (c) => {
  const birth = new Date(c.birthDate);
  return {
    id:c.childID, name:`${c.firstName} ${c.middleName||""} ${c.lastName}`.trim(),
    birthDate:birth.toLocaleDateString("en-US",{year:"numeric",month:"short",day:"numeric"}),
    birthDateRaw:c.birthDate, // ISO value straight from the API, used for editing/saving
    birthPlace:c.placeOfBirth||"—", age:calculateAge(birth), sex:c.sex||"—",
    height:c.birthHeight?`${c.birthHeight} cm`:"—", weight:c.birthWeight?`${c.birthWeight} kg`:"—",
    // NOTE: no vaccination or status field/endpoint exists in ChildrenController — hardcoded until the API exposes one.
    vaccStatus:"Upcoming", nextVaccine:"—", status:"Active", address:c.address||"—", barangay:c.barangay||"—", notifyMode:"primary",
    raw:c, // original API record, kept so edits that don't touch every field don't lose data
  };
};


async function loadAll() {
    console.log("loadAll started");

    isLoading.value = true;

    try {
        const [pd, cd] = await Promise.all([
            api.getAllParents(),
            api.getAllChildren()
        ]);

        console.log("typeof parents:", typeof pd);
        console.log("parents raw:", pd);
        console.log("Array?", Array.isArray(pd));

        console.log("typeof children:", typeof cd);
        console.log("children raw:", cd);
        console.log("Array?", Array.isArray(cd));

        parents.value = Array.isArray(pd)
            ? pd.map(mapParent)
            : [];

        children.value = Array.isArray(cd)
            ? cd.map(mapChild)
            : [];

        // Load actual parent-child links after children are available.
        await loadRelationships();
    }
    catch (e) {
        console.error(e);
    }
    finally {
        isLoading.value = false;
    }
}





// Relationships are loaded from the real ChildParentRelationships API.
// The old demo REL-001...REL-005 records have intentionally been removed.
const relationships = ref([]);

function mapRelationship(r) {
  return {
    id: r.relationshipID,
    parentId: r.parentID,
    childId: r.childID,
    relationship: r.relationshipType,
    isPrimary: !!r.isPrimaryContact,
    canReceiveNotifications: r.canReceiveNotifications !== false,
    status: r.status || "Active",
    createdDate: r.createdAt
      ? new Date(r.createdAt).toLocaleDateString("en-US", {
          year: "numeric",
          month: "short",
          day: "numeric"
        })
      : "—",
    raw: r,
  };
}

async function loadRelationships() {
  if (!children.value.length) {
    relationships.value = [];
    return;
  }

  try {
    const results = await Promise.all(
      children.value.map(child => api.getRelationshipsByChild(child.id))
    );

    relationships.value = results
      .flatMap(result => Array.isArray(result) ? result : [])
      .map(mapRelationship)
      .filter(r => r.status === "Active");

    console.log("Relationships loaded from API:", relationships.value);
  } catch (e) {
    console.error("Error loading parent-child relationships:", e);
    relationships.value = [];
  }
}

/* -- relationship helpers: everything about "who is linked to whom" flows through these -- */
const relationshipsOfChild = (child) =>
  relationships.value.filter(r => r.childId === child.id && r.status === "Active");

const relationshipsOfParent = (parent) =>
  relationships.value.filter(r => r.parentId === parent.id && r.status === "Active");

const linkedAccountsOf = (child) => relationshipsOfChild(child)
  .map(rel => ({
    rel,
    parent: parents.value.find(p => p.id === rel.parentId)
  }))
  .filter(x => x.parent)
  .sort((a, b) => (b.rel.isPrimary ? 1 : 0) - (a.rel.isPrimary ? 1 : 0));

const childrenOf = (parent) => relationshipsOfParent(parent)
  .map(rel => ({
    rel,
    child: children.value.find(c => c.id === rel.childId)
  }))
  .filter(x => x.child);

function setPrimary(childId, relId) {
  // The current backend does not yet expose a PUT/PATCH endpoint for
  // changing IsPrimaryContact. Keep this as a local UI change for now.
  relationships.value.forEach(r => {
    if (r.childId === childId && r.status === "Active") {
      r.isPrimary = r.id === relId;
    }
  });
}

/* ============================= Summary cards (per tab) ============================= */
const count = (arr, pred) => arr.filter(pred).length;
const parentSummary = computed(() => [
  { label:"Total Parents", value:parents.value.length, tint:"text-emerald-700", tintBg:"bg-emerald-50", icon:Users },
  { label:"Active Parents", value:count(parents.value,p=>p.status==="Active"), tint:"text-emerald-700", tintBg:"bg-emerald-50", icon:Check },
  { label:"Inactive Parents", value:count(parents.value,p=>p.status==="Inactive"), tint:"text-stone-500", tintBg:"bg-stone-100", icon:Ban },
  { label:"Pending Accounts", value:count(parents.value,p=>p.status==="Pending"), tint:"text-amber-700", tintBg:"bg-amber-50", icon:UserPlus },
]);
const childSummary = computed(() => [
  { label:"Total Children", value:children.value.length, tint:"text-emerald-700", tintBg:"bg-emerald-50", icon:Baby },
  { label:"Fully Vaccinated", value:count(children.value,c=>c.vaccStatus==="Fully Vaccinated"), tint:"text-emerald-700", tintBg:"bg-emerald-50", icon:Syringe },
  { label:"Partially Vaccinated", value:count(children.value,c=>c.vaccStatus==="Partially Vaccinated"), tint:"text-amber-700", tintBg:"bg-amber-50", icon:Syringe },
  { label:"No Linked Account", value:count(children.value,c=>relationshipsOfChild(c).length===0), tint:"text-rose-700", tintBg:"bg-rose-50", icon:Link2 },
]);

/* ============================= Search ============================= */
const searchQuery = ref("");
const filteredParents = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  return q ? parents.value.filter(p=>p.name.toLowerCase().includes(q)||p.id.toLowerCase().includes(q)||p.contact.includes(q)) : parents.value;
});
const filteredChildren = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  return q ? children.value.filter(c=>c.name.toLowerCase().includes(q)||c.id.toLowerCase().includes(q)) : children.value;
});

/* =========================================================================
   TABLE COLUMN CONFIGS — this is the big compression win. Instead of two
   full hand-written <table> markups (one per tab), each table is just a
   v-for over one of these arrays. "badge" picks a special renderer,
   "muted" applies the gray text style, "value" computes derived cells.
   Add/remove a column here and both header + body update automatically.
========================================================================= */
const parentColumns = [
  { key:"avatar", badge:"avatar" },
  { key:"id", label:"Parent ID" },
  { key:"name", label:"Full Name" },
  { key:"relationship", label:"Relationship", muted:true },
  { key:"contact", label:"Contact Number", muted:true },
  { key:"username", label:"Username", muted:true },
  { key:"status", label:"Status", badge:"status" },
  { key:"childrenCount", label:"Children Linked", muted:true, value:(p)=>relationshipsOfParent(p).length },
];
const childColumns = [
  { key:"avatar", badge:"childIcon" },
  { key:"id", label:"Patient ID" },
  { key:"name", label:"Child Name" },
  { key:"age", label:"Age", muted:true },
  { key:"sex", label:"Sex", muted:true },
  { key:"vaccStatus", label:"Vaccination Status", badge:"vacc" },
  { key:"linked", label:"Linked Accounts", badge:"linked" },
  { key:"nextVaccine", label:"Next Vaccine", muted:true },
  { key:"status", label:"Status", badge:"status" },
];

/* -- row "..." action menus, same idea: config array instead of two hand-written dropdowns -- */
const parentMenuItems = [
  { icon:Baby, label:"Register Child", action:(p)=>{ selectedParent.value=p; openRegister("child"); } },
  { icon:Link2, label:"Link Existing Child", action:(p)=>openPicker("linkChild",{parent:p}) },
  { divider:true },
  { icon:KeyRound, label:"Reset Password", action:()=>{} },
  { icon:Ban, label:"Deactivate", class:"text-amber-700 hover:bg-amber-50", action:()=>{} },
  { icon:ArchiveIcon, label:"Archive", class:"text-rose-700 hover:bg-rose-50", action:()=>{} },
];
const childMenuItems = [
  { icon:Link2, label:"Link Parent / Guardian", action:(c)=>openPicker("linkParent",{child:c}) },
  { icon:UserCircle2, label:"Manage Linked Accounts", action:(c)=>viewChild(c) },
  { icon:Syringe, label:"Add Historical Vaccination Records", action:(c)=>{ selectedChild.value=c; loadChildVaccinations(c.id); openHistoricalModal(); } },
  { divider:true },
  { icon:ArchiveIcon, label:"Archive", class:"text-rose-700 hover:bg-rose-50", action:()=>{} },
];

const actionsMenuOpenFor = ref(null);
const toggleActionsMenu = (id) => { actionsMenuOpenFor.value = actionsMenuOpenFor.value===id ? null : id; };

/* ============================= View drawers ============================= */
const showParentDrawer = ref(false);
const selectedParent = ref(null);
function viewParent(p) { selectedParent.value=p; showParentDrawer.value=true; actionsMenuOpenFor.value=null; }

const showChildDrawer = ref(false);
const selectedChild = ref(null);
const removeConfirmId = ref(null); // relationship id currently showing the "remove?" confirm buttons

/* ============================= Vaccination history (per child) ============================= */
const vaccinationRecordsByChild = ref({}); // { [childId]: VaccinationRecordDTO[] }
const vaccinationLoading = ref(false);
const selectedChildVaccinations = computed(() => vaccinationRecordsByChild.value[selectedChild.value?.id] || []);

function formatVaccDate(iso) {
  if (!iso) return "—";
  const d = new Date(iso);
  return isNaN(d) ? String(iso).slice(0,10) : d.toLocaleDateString("en-US",{year:"numeric",month:"short",day:"numeric"});
}

async function loadChildVaccinations(childId) {
  if (!childId) return;
  vaccinationLoading.value = true;
  try {
    const data = await api.getChildVaccinations(childId);
    vaccinationRecordsByChild.value = { ...vaccinationRecordsByChild.value, [childId]: Array.isArray(data) ? data : [] };
  } catch (e) {
    console.error("Error loading vaccination records:", e);
  } finally {
    vaccinationLoading.value = false;
  }
}

/* -- Historical Vaccination Records modal -- */
function emptyHistoricalRow() { return { vaccineID:"", doseNumber:1, vaccinationDate:"" }; }
const historicalModal = ref({ open:false, rows:[emptyHistoricalRow()], error:null, submitting:false, success:false });

function openHistoricalModal() {
  historicalModal.value = { open:true, rows:[emptyHistoricalRow()], error:null, submitting:false, success:false };
  actionsMenuOpenFor.value = null;
}
const closeHistoricalModal = () => (historicalModal.value.open = false);
const addHistoricalRow = () => historicalModal.value.rows.push(emptyHistoricalRow());
function removeHistoricalRow(idx) {
  historicalModal.value.rows.splice(idx, 1);
  if (historicalModal.value.rows.length === 0) historicalModal.value.rows.push(emptyHistoricalRow());
}

async function submitHistoricalVaccinations() {
  const m = historicalModal.value;
  m.error = null;

  if (!selectedChild.value?.id) { m.error = "Select a child first."; return; }
  if (!m.rows.length) { m.error = "Add at least one vaccination record."; return; }

  for (const row of m.rows) {
    if (!row.vaccineID) { m.error = "Every row needs a vaccine selected."; return; }
    if (!row.doseNumber || Number(row.doseNumber) <= 0) { m.error = "Dose number must be greater than 0."; return; }
    if (!row.vaccinationDate) { m.error = "Every row needs a vaccination date."; return; }
  }
  // prevent obvious accidental duplicates: same vaccine + dose + date entered twice in this submission
  const seen = new Set();
  for (const row of m.rows) {
    const key = `${row.vaccineID}-${row.doseNumber}-${row.vaccinationDate}`;
    if (seen.has(key)) { m.error = "Duplicate vaccine/dose/date rows found — please review your entries."; return; }
    seen.add(key);
  }

  m.submitting = true;
  try {
    await api.saveHistoricalVaccinations({
      childID: selectedChild.value.id,
      vaccinations: m.rows.map(row => ({
        vaccineID: Number(row.vaccineID),
        doseNumber: Number(row.doseNumber),
        vaccinationDate: row.vaccinationDate,
      })),
    });
    m.success = true;
    m.rows = [emptyHistoricalRow()];
    await loadChildVaccinations(selectedChild.value.id);
  } catch (e) {
    console.error("Error saving historical vaccinations:", e);
    m.error = `Save failed: ${e.response?.data?.message || e.message}`;
  } finally {
    m.submitting = false;
  }
}
function viewChild(c) { selectedChild.value=c; showChildDrawer.value=true; actionsMenuOpenFor.value=null; removeConfirmId.value=null; loadChildVaccinations(c.id); }
const askRemoveAccount = (rel) => (removeConfirmId.value = rel.id);
const cancelRemoveAccount = () => (removeConfirmId.value = null);

async function confirmRemoveAccount(rel) {
  try {
    await api.deleteRelationship(rel.id);
    await loadRelationships();
    removeConfirmId.value = null;
  } catch (e) {
    console.error("Error removing parent-child relationship:", e);
    error.value = `Remove failed: ${e.response?.data?.message || e.message}`;
    removeConfirmId.value = null;
  }
}

/* -- Linked Accounts quick-view modal (from the child table cell) -- */
const showLinkedAccountsModal = ref(false);
const viewingChildAccounts = ref(null);
const openLinkedAccountsModal = (c) => { viewingChildAccounts.value=c; showLinkedAccountsModal.value=true; };
const manageFromLinkedAccountsModal = () => { showLinkedAccountsModal.value=false; viewChild(viewingChildAccounts.value); };

/* =========================================================================
   GENERIC PICKER MODAL — replaces "Link Existing Child" / "Link Parent-
   Guardian" / "Transfer Guardian". picker.mode drives behavior; .parent /
   .child / .rel carry whatever context the caller had.
========================================================================= */
const PICKER_CONFIG = {
  linkChild:  { title:"Link Existing Child",   searchPlaceholder:"Search by Patient ID, Child Name, or Birth Date...", showPrimary:true },
  linkParent: { title:"Link Parent / Guardian", searchPlaceholder:"Search by Parent ID, Name, Phone, or Email...",     showPrimary:true },
  transfer:   { title:"Transfer Guardian",      searchPlaceholder:"Search for a parent account...",                    showPrimary:false },
};
const picker = ref({
  open:false,
  mode:null,
  query:"",
  selected:null,
  relationship:"",
  isPrimary:false,
  parent:null,
  child:null,
  rel:null,
  submitting:false,
  error:null
});

function openPicker(mode, ctx={}) {
  picker.value = {
    open:true,
    mode,
    query:"",
    selected:null,
    relationship:"",
    isPrimary:false,
    parent:null,
    child:null,
    rel:null,
    submitting:false,
    error:null,
    ...ctx
  };
  actionsMenuOpenFor.value=null;
}

const closePicker = () => {
  if (picker.value.submitting) return;
  picker.value.open = false;
};

const pickerResults = computed(() => {
  const s = picker.value, q = s.query.trim().toLowerCase();
  if (s.mode==="linkChild") {
    const linked = s.parent ? relationshipsOfParent(s.parent).map(r=>r.childId) : [];
    const list = children.value.filter(c=>!linked.includes(c.id));
    return q ? list.filter(c=>c.name.toLowerCase().includes(q)||c.id.toLowerCase().includes(q)||c.birthDate.toLowerCase().includes(q)) : list;
  }
  if (s.mode==="linkParent") {
    const linked = s.child ? relationshipsOfChild(s.child).map(r=>r.parentId) : [];
    const list = parents.value.filter(p=>!linked.includes(p.id));
    return q ? list.filter(p=>p.name.toLowerCase().includes(q)||p.id.toLowerCase().includes(q)||p.contact.includes(q)||p.email.toLowerCase().includes(q)) : list;
  }
  if (s.mode==="transfer") {
    const list = parents.value.filter(p=>p.id!==s.rel?.parentId);
    return q ? list.filter(p=>p.name.toLowerCase().includes(q)||p.id.toLowerCase().includes(q)||p.contact.includes(q)) : list;
  }
  return [];
});
async function confirmPicker() {
  const s = picker.value;

  if (!s.selected || !s.relationship) return;

  s.error = null;
  s.submitting = true;

  try {
    if (s.mode === "transfer") {
      // Transfer = deactivate old relationship, then create the new one.
      await api.deleteRelationship(s.rel.id);

      await api.createRelationship({
        childID: s.rel.childId,
        parentID: s.selected.id,
        relationshipType: s.relationship,
        isPrimaryContact: !!s.rel.isPrimary,
        canReceiveNotifications: s.rel.canReceiveNotifications !== false
      });
    } else {
      const parentId = s.mode === "linkChild"
        ? s.parent.id
        : s.selected.id;

      const childId = s.mode === "linkChild"
        ? s.selected.id
        : s.child.id;

      // This is the real API call that creates the link.
      await api.createRelationship({
        childID: childId,
        parentID: parentId,
        relationshipType: s.relationship,
        isPrimaryContact: !!s.isPrimary,
        canReceiveNotifications: true
      });
    }

    // Re-read the database so the UI is never relying on fake/local links.
    await loadRelationships();

    // Keep the currently opened child drawer showing the fresh relationship list.
    if (selectedChild.value?.id) {
      const freshChild = children.value.find(c => c.id === selectedChild.value.id);
      if (freshChild) selectedChild.value = freshChild;
    }

    closePicker();
  } catch (e) {
    console.error("Error saving parent-child relationship:", e);
    s.error = `Link failed: ${e.response?.data?.message || e.message}`;
  } finally {
    s.submitting = false;
  }
}

/* =========================================================================
   GENERIC REGISTER MODAL (two-stage: form -> success). "Register Parent"
   and "Register Child" share one form, driven by FORM_FIELDS.parent/.child.
========================================================================= */
const FORM_FIELDS = {
  parent: [
    { key:"name", label:"Full Name", full:true }, { key:"contact", label:"Contact Number" }, { key:"email", label:"Email" },
    { key:"password", label:"Password", type:"password", full:true }, { key:"address", label:"Address", full:true }, { key:"barangay", label:"Barangay", full:true },
  ],
  child: [
    { key:"name", label:"Child Name", full:true }, { key:"birthDate", label:"Birth Date", type:"date" }, { key:"birthPlace", label:"Birth Place" },
    { key:"sex", type:"select", placeholder:"Sex", options:["Male","Female"] }, { key:"address", label:"Address", full:true }, { key:"barangay", label:"Barangay", full:true },
  ],
};
const registerModal = ref({ open:false, kind:null, stage:"form", formData:{} });
const openRegister = (kind) => { registerModal.value = { open:true, kind, stage:"form", formData:{} }; actionsMenuOpenFor.value=null; };
const closeRegister = () => (registerModal.value.open = false);

async function submitRegister() {
  const kind = registerModal.value.kind, fd = registerModal.value.formData;
  try {
    if (kind==="parent") {
      if (!fd.name || !fd.email || !fd.contact || !fd.password) { error.value="Please fill all required parent fields"; return; }
      const created = await api.createParent({
        firstName:fd.name.split(" ")[0], lastName:fd.name.split(" ").slice(1).join(" ")||"—",
        email:fd.email, contactNo:fd.contact, address:fd.address||"", barangayNo:fd.barangay||"", password:fd.password,
      });
      await loadAll();
      // so the success dialog's "Register Child" / "Link Existing Child" buttons act on the parent just created
      selectedParent.value = mapParent(created);
    } else {
      if (!fd.name || !fd.birthDate) { error.value="Please fill all required child fields"; return; }
      // ChildrenController requires a non-empty ParentID — this form has no parent picker of its own,
      // so a child can only be registered from a parent's row ("Register Child" action).
      if (!selectedParent.value?.id) { error.value = "Select a parent first — use “Register Child” from a parent’s row."; return; }
      const created = await api.createChild({
        firstName:fd.name.split(" ")[0], lastName:fd.name.split(" ").slice(1).join(" ")||"—", middleName:"",
        birthDate:fd.birthDate, placeOfBirth:fd.birthPlace||"", sex:fd.sex||"",
        barangay:fd.barangay ? Number(fd.barangay) : null, // Barangay is an int in the DB — see note below
        address:fd.address||"", parentID:selectedParent.value.id,
      });
      await loadAll();
      // so the success dialog's "Link Existing Parent" button acts on the child just created
      selectedChild.value = mapChild(created);
    }
    registerModal.value.stage = "success"; error.value = null;
  } catch (e) { console.error("Error submitting registration:", e); error.value = `Registration failed: ${e.response?.data?.message || e.message}`; }
}

/* =========================================================================
   GENERIC EDIT MODAL — replaces Edit Parent / Edit Child. Fields v-model
   onto a working copy (editModal.item).
========================================================================= */
const EDIT_FIELDS = {
  parent: [
    { key:"name", label:"Full Name" }, { key:"relationship", label:"Relationship", type:"select", options:relationshipOptions },
    { key:"contact", label:"Contact Number" }, { key:"email", label:"Email" }, { key:"address", label:"Address" },
  ],
  child: [
    { key:"name", label:"Name" }, { key:"birthDate", label:"Birth Date" },
    { key:"height", label:"Height", half:true }, { key:"weight", label:"Weight", half:true }, { key:"address", label:"Address" },
  ],
};
const editModal = ref({ open:false, kind:null, item:null });
const openEdit = (kind, item) => { editModal.value = { open:true, kind, item:{...item} }; actionsMenuOpenFor.value=null; };

async function submitEdit() {
  const kind = editModal.value.kind, item = editModal.value.item;
  try {
    if (kind==="parent") {
      await api.updateParent(item.id, {
        firstName:item.name.split(" ")[0], lastName:item.name.split(" ").slice(1).join(" ")||"—",
        email:item.email, contactNo:item.contact, address:item.address, barangayNo:item.barangay||"",
      });
    } else {
      const raw = item.raw || {};
      // UpdateChild (see ChildrenController) overwrites every column from the DTO, so fields this
      // form doesn't edit are re-sent from the last-known raw record to avoid wiping them out.
      await api.updateChild(item.id, {
        firstName:item.name.split(" ")[0], lastName:item.name.split(" ").slice(1).join(" ")||"—",
        middleName:raw.middleName||"", birthDate:item.birthDateRaw||raw.birthDate,
        placeOfBirth:raw.placeOfBirth||"", sex:raw.sex||"", barangay:raw.barangay ?? null,
        birthHeight:parseFloat(item.height) || raw.birthHeight || null,
        birthWeight:parseFloat(item.weight) || raw.birthWeight || null,
        address:item.address, healthCenter:raw.healthCenter||"",
        motherName:raw.motherName||"", fatherName:raw.fatherName||"", guardianName:raw.guardianName||"",
      });
    }
    await loadAll();
    editModal.value.open = false; error.value = null;
  } catch (e) { console.error("Error updating record:", e); error.value = `Update failed: ${e.response?.data?.message || e.message}`; }
}
// Groups fields into rows of 2 when both are marked `half` (e.g. Height + Weight side by side)
function fieldRows(fields) {
  const rows = [];
  for (let i=0; i<fields.length; i++) {
    if (fields[i].half && fields[i+1]?.half) { rows.push([fields[i], fields[i+1]]); i++; }
    else rows.push([fields[i]]);
  }
  return rows;
}
</script>

<template>
  <div class="flex min-h-screen w-full bg-stone-50 text-stone-900" style="font-family: 'Inter','Segoe UI',sans-serif;">
    <!-- ---------------- Sidebar ---------------- -->
    <aside class="flex flex-col shrink-0 border-r border-stone-200 bg-white transition-all duration-200" :class="collapsed ? 'w-[76px]' : 'w-[264px]'">
      <div class="flex items-center gap-3 px-5 py-5 border-b border-stone-200">
        <div class="flex h-9 w-9 items-center justify-center rounded-xl text-white font-bold text-sm shrink-0 bg-gradient-to-br from-emerald-600 to-emerald-800">A</div>
        <div v-if="!collapsed" class="leading-tight">
          <p class="font-semibold text-[15px]">Aruga Pediatric System</p>
          <p class="text-[11px] text-stone-500">Staff Portal</p>
        </div>
      </div>
      <nav class="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
        <button v-for="item in navItems" :key="item.label" class="flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-[13.5px] font-medium transition-colors" :class="item.active ? 'bg-emerald-50 text-emerald-800' : 'text-stone-500 hover:bg-stone-50'">
          <component :is="item.icon" :size="18" :stroke-width="2" /><span v-if="!collapsed">{{ item.label }}</span>
        </button>
      </nav>
      <div class="border-t border-stone-200 px-3 py-4">
        <div class="flex items-center gap-3 rounded-xl px-2 py-2">
          <div class="flex h-9 w-9 items-center justify-center rounded-full text-white text-xs font-semibold shrink-0 bg-sky-700">MP</div>
          <div v-if="!collapsed" class="leading-tight"><p class="text-[13px] font-semibold">Marielle Pascual</p><p class="text-[11px] text-stone-500">Clinic Staff</p></div>
        </div>
        <button class="mt-2 flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-[13px] font-medium text-rose-700 hover:bg-rose-50"><LogOut :size="17" /><span v-if="!collapsed">Log out</span></button>
      </div>
      <button @click="collapsed = !collapsed" class="mx-auto mb-3 flex h-7 w-7 items-center justify-center rounded-full border border-stone-200 text-stone-500">
        <component :is="collapsed ? ChevronRight : ChevronLeft" :size="14" />
      </button>
    </aside>

    <!-- ---------------- Main ---------------- -->
    <main class="flex-1 min-w-0">
      <header class="flex items-center justify-between px-8 py-5 border-b border-stone-200 bg-white">
        <div><h1 class="text-[22px] font-bold">Patient Management</h1><p class="text-[12.5px] mt-0.5 text-stone-500">Dashboard &gt; Patient Management</p></div>
        <div class="flex items-center gap-3">
          <button class="relative flex h-9 w-9 items-center justify-center rounded-full bg-stone-50"><Bell :size="17" class="text-stone-500" /><span class="absolute top-1.5 right-2 h-1.5 w-1.5 rounded-full bg-rose-600" /></button>
          <button class="flex h-9 w-9 items-center justify-center rounded-full bg-stone-50"><Settings :size="17" class="text-stone-500" /></button>
          <div class="flex h-9 w-9 items-center justify-center rounded-full text-white text-xs font-semibold bg-emerald-700">MP</div>
        </div>
      </header>

      <div class="px-8 py-6 space-y-6">
        <!-- Summary cards -->
        <div class="grid grid-cols-4 gap-4">
          <div v-for="c in (activeTab === 'parents' ? parentSummary : childSummary)" :key="c.label" class="rounded-2xl border border-stone-200 bg-white p-4 shadow-sm">
            <div class="flex items-center justify-between">
              <p class="text-[10.5px] font-semibold uppercase tracking-wide text-stone-500 leading-tight">{{ c.label }}</p>
              <div class="flex h-7 w-7 items-center justify-center rounded-lg shrink-0" :class="c.tintBg"><component :is="c.icon" :size="14" :class="c.tint" /></div>
            </div>
            <p class="text-[24px] font-bold mt-2">{{ c.value }}</p>
          </div>
        </div>

        <!-- Tabs + toolbar -->
        <div class="rounded-2xl border border-stone-200 bg-white p-4 shadow-sm">
          <div class="flex flex-wrap items-center gap-3">
            <div class="flex items-center rounded-xl border border-stone-200 p-1">
              <button @click="activeTab = 'parents'; searchQuery = ''" class="flex items-center gap-1.5 rounded-lg px-4 py-1.5 text-[12.5px] font-medium" :class="activeTab === 'parents' ? 'bg-emerald-700 text-white' : 'text-stone-500'"><UserCircle2 :size="14" /> Parents</button>
              <button @click="activeTab = 'children'; searchQuery = ''" class="flex items-center gap-1.5 rounded-lg px-4 py-1.5 text-[12.5px] font-medium" :class="activeTab === 'children' ? 'bg-emerald-700 text-white' : 'text-stone-500'"><Baby :size="14" /> Children</button>
            </div>
            <div class="flex items-center gap-2 flex-1 min-w-[240px] rounded-xl border border-stone-200 px-3 py-2.5">
              <Search :size="16" class="text-stone-400 shrink-0" />
              <input v-model="searchQuery" type="text" :placeholder="activeTab === 'parents' ? 'Search by name, parent ID, or contact...' : 'Search by name or patient ID...'" class="flex-1 text-[13px] outline-none placeholder:text-stone-400" />
            </div>
            <div class="flex-1"></div>
            <button v-if="activeTab === 'parents'" @click="openRegister('parent')" class="flex items-center gap-2 rounded-xl px-4 py-2.5 text-[13px] font-semibold text-white bg-emerald-700 hover:bg-emerald-800 shadow-sm"><UserPlus :size="16" /> Register Parent</button>
            <button v-else @click="openRegister('child')" class="flex items-center gap-2 rounded-xl px-4 py-2.5 text-[13px] font-semibold text-white bg-emerald-700 hover:bg-emerald-800 shadow-sm"><UserPlus :size="16" /> Register Child</button>
          </div>
        </div>

        <!-- ============== PARENTS TABLE (columns driven by parentColumns) ============== -->
        <div v-if="activeTab === 'parents'" class="rounded-2xl border border-stone-200 bg-white shadow-sm overflow-visible">
          <div class="flex items-center justify-between px-5 py-4 border-b border-stone-200"><p class="text-[14px] font-semibold">Parent Directory</p><span class="text-[12px] text-stone-500">{{ filteredParents.length }} parents</span></div>
          <div class="overflow-x-auto">
            <table class="w-full text-[13px]">
              <thead><tr class="bg-stone-50">
                <th v-for="col in parentColumns" :key="col.key" class="text-left font-semibold px-4 py-3 whitespace-nowrap text-[11px] uppercase tracking-wide text-stone-500">{{ col.label }}</th>
                <th></th>
              </tr></thead>
              <tbody>
                <tr v-for="p in filteredParents" :key="p.id" class="border-t border-stone-200">
                  <td v-for="col in parentColumns" :key="col.key" class="whitespace-nowrap" :class="[col.muted && 'text-stone-500', col.badge==='avatar' ? 'pl-5 pr-2 py-3' : 'px-4 py-3']">
                    <Avatar v-if="col.badge==='avatar'" :text="p.initials" size="sm" />
                    <StatusBadge v-else-if="col.badge==='status'" :status="p.status" />
                    <template v-else>{{ col.value ? col.value(p) : p[col.key] }}</template>
                  </td>
                  <td class="px-4 py-3 whitespace-nowrap">
                    <div class="flex items-center gap-1 relative">
                      <button @click="viewParent(p)" class="p-1.5 rounded-lg hover:bg-stone-100" title="View Details"><Eye :size="15" class="text-stone-500" /></button>
                      <button @click="openEdit('parent', p)" class="p-1.5 rounded-lg hover:bg-stone-100" title="Edit Parent"><Pencil :size="15" class="text-stone-500" /></button>
                      <button @click="toggleActionsMenu(p.id)" class="p-1.5 rounded-lg hover:bg-stone-100" title="More actions"><MoreHorizontal :size="15" class="text-stone-500" /></button>
                      <div v-if="actionsMenuOpenFor === p.id" class="absolute right-0 top-9 z-10 w-56 rounded-xl border border-stone-200 bg-white shadow-lg py-1.5">
                        <template v-for="(m,i) in parentMenuItems" :key="i">
                          <div v-if="m.divider" class="my-1 border-t border-stone-100"></div>
                          <button v-else @click="m.action(p)" class="flex w-full items-center gap-2.5 px-3.5 py-2 text-[12.5px] hover:bg-stone-50" :class="m.class || 'text-stone-700'">
                            <component :is="m.icon" :size="15" :class="m.class ? '' : 'text-stone-500'" /> {{ m.label }}
                          </button>
                        </template>
                      </div>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- ============== CHILDREN TABLE (columns driven by childColumns) ============== -->
        <div v-else class="rounded-2xl border border-stone-200 bg-white shadow-sm overflow-visible">
          <div class="flex items-center justify-between px-5 py-4 border-b border-stone-200"><p class="text-[14px] font-semibold">Child Directory</p><span class="text-[12px] text-stone-500">{{ filteredChildren.length }} children</span></div>
          <div class="overflow-x-auto">
            <table class="w-full text-[13px]">
              <thead><tr class="bg-stone-50">
                <th v-for="col in childColumns" :key="col.key" class="text-left font-semibold px-4 py-3 whitespace-nowrap text-[11px] uppercase tracking-wide text-stone-500">{{ col.label }}</th>
                <th></th>
              </tr></thead>
              <tbody>
                <tr v-for="ch in filteredChildren" :key="ch.id" class="border-t border-stone-200">
                  <td v-for="col in childColumns" :key="col.key" class="whitespace-nowrap" :class="[col.muted && 'text-stone-500', col.badge==='childIcon' ? 'pl-5 pr-2 py-3' : 'px-4 py-3']">
                    <div v-if="col.badge==='childIcon'" class="flex h-9 w-9 items-center justify-center rounded-full bg-emerald-50"><Baby :size="16" class="text-emerald-700" /></div>
                    <VaccBadge v-else-if="col.badge==='vacc'" :status="ch.vaccStatus" />
                    <StatusBadge v-else-if="col.badge==='status'" :status="ch.status" />
                    <template v-else-if="col.badge==='linked'">
                      <button v-if="linkedAccountsOf(ch).length > 0" @click="openLinkedAccountsModal(ch)" class="text-left rounded-lg -mx-1 px-1 py-0.5 hover:bg-stone-50 transition-colors">
                        <span class="block text-stone-700">{{ linkedAccountsOf(ch)[0].rel.relationship }}: {{ linkedAccountsOf(ch)[0].parent.name }}</span>
                        <span v-if="linkedAccountsOf(ch).length > 1" class="block text-[11.5px] font-medium text-emerald-700">+{{ linkedAccountsOf(ch).length - 1 }} more</span>
                      </button>
                      <button v-else @click="viewChild(ch)" class="inline-flex items-center gap-1 text-rose-700 text-[12px] font-medium hover:underline"><Link2 :size="12" /> Not linked</button>
                    </template>
                    <template v-else>{{ col.value ? col.value(ch) : ch[col.key] }}</template>
                  </td>
                  <td class="px-4 py-3 whitespace-nowrap">
                    <div class="flex items-center gap-1 relative">
                      <button @click="viewChild(ch)" class="p-1.5 rounded-lg hover:bg-stone-100" title="View Child"><Eye :size="15" class="text-stone-500" /></button>
                      <button @click="openEdit('child', ch)" class="p-1.5 rounded-lg hover:bg-stone-100" title="Edit Child"><Pencil :size="15" class="text-stone-500" /></button>
                      <button @click="toggleActionsMenu(ch.id)" class="p-1.5 rounded-lg hover:bg-stone-100" title="More actions"><MoreHorizontal :size="15" class="text-stone-500" /></button>
                      <div v-if="actionsMenuOpenFor === ch.id" class="absolute right-0 top-9 z-10 w-56 rounded-xl border border-stone-200 bg-white shadow-lg py-1.5">
                        <template v-for="(m,i) in childMenuItems" :key="i">
                          <div v-if="m.divider" class="my-1 border-t border-stone-100"></div>
                          <button v-else @click="m.action(ch)" class="flex w-full items-center gap-2.5 px-3.5 py-2 text-[12.5px] hover:bg-stone-50" :class="m.class || 'text-stone-700'">
                            <component :is="m.icon" :size="15" :class="m.class ? '' : 'text-stone-500'" /> {{ m.label }}
                          </button>
                        </template>
                      </div>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </main>

    <!-- Linked Accounts quick-view modal -->
    <div v-if="showLinkedAccountsModal && viewingChildAccounts" class="fixed inset-0 z-50 flex items-center justify-center bg-stone-900/40 px-4">
      <div class="w-full max-w-md rounded-2xl bg-white shadow-xl">
        <div class="flex items-center justify-between px-6 py-4 border-b border-stone-200">
          <div><p class="text-[15px] font-semibold">Linked Parent / Guardian Accounts</p><p class="text-[11.5px] text-stone-500">{{ viewingChildAccounts.name }} · {{ viewingChildAccounts.id }}</p></div>
          <button @click="showLinkedAccountsModal = false" class="p-1.5 rounded-lg hover:bg-stone-100"><X :size="18" class="text-stone-500" /></button>
        </div>
        <div class="p-6 space-y-2 max-h-[60vh] overflow-y-auto">
          <div v-for="a in linkedAccountsOf(viewingChildAccounts)" :key="a.rel.id" class="flex items-center gap-3 rounded-xl border border-stone-200 p-3">
            <Avatar :text="a.parent.initials" size="md" />
            <div class="flex-1 min-w-0">
              <p class="text-[12.5px] font-semibold flex items-center gap-1.5">{{ a.parent.name }} <Star v-if="a.rel.isPrimary" :size="12" class="text-amber-500 fill-amber-500" /></p>
              <p class="text-[11px] text-stone-500">{{ a.rel.relationship }} · {{ a.parent.contact }}</p>
            </div>
            <span v-if="a.rel.isPrimary" class="text-[10.5px] font-medium text-amber-700 bg-amber-50 rounded-full px-2 py-1 shrink-0">Primary</span>
          </div>
        </div>
        <div class="flex items-center justify-end gap-3 px-6 py-4 border-t border-stone-200">
          <button @click="showLinkedAccountsModal = false" class="rounded-xl px-4 py-2.5 text-[13px] font-medium text-stone-600 hover:bg-stone-50">Close</button>
          <button @click="manageFromLinkedAccountsModal" class="rounded-xl px-5 py-2.5 text-[13px] font-semibold text-white bg-emerald-700 hover:bg-emerald-800 shadow-sm">Manage Accounts</button>
        </div>
      </div>
    </div>

    <!-- VIEW PARENT DRAWER -->
    <transition name="fade"><div v-if="showParentDrawer" class="fixed inset-0 z-30 bg-stone-900/30" @click="showParentDrawer = false"></div></transition>
    <transition name="slide">
      <aside v-if="showParentDrawer && selectedParent" class="fixed right-0 top-0 z-40 h-full w-[440px] bg-white border-l border-stone-200 shadow-xl overflow-y-auto">
        <div class="flex items-center justify-between px-5 py-4 border-b border-stone-200 sticky top-0 bg-white">
          <div><p class="text-[15px] font-semibold">Parent Information</p><p class="text-[11.5px] text-stone-500">{{ selectedParent.id }}</p></div>
          <button @click="showParentDrawer = false" class="p-1.5 rounded-lg hover:bg-stone-100"><X :size="17" class="text-stone-500" /></button>
        </div>
        <div class="p-5 space-y-6">
          <div class="flex items-center gap-4">
            <Avatar :text="selectedParent.initials" size="lg" />
            <div><p class="text-[16px] font-bold">{{ selectedParent.name }}</p><p class="text-[12px] text-stone-500">{{ selectedParent.relationship }}</p><div class="mt-1"><StatusBadge :status="selectedParent.status" /></div></div>
          </div>
          <div class="rounded-xl bg-stone-50 p-3.5 space-y-2">
            <div class="flex items-center justify-between"><span class="text-[12px] text-stone-500">Contact Number</span><span class="text-[13px] font-medium flex items-center gap-1.5"><Phone :size="13" class="text-stone-400" /> {{ selectedParent.contact }}</span></div>
            <div class="flex items-center justify-between"><span class="text-[12px] text-stone-500">Email</span><span class="text-[13px] font-medium flex items-center gap-1.5"><Mail :size="13" class="text-stone-400" /> {{ selectedParent.email }}</span></div>
            <div class="flex items-center justify-between"><span class="text-[12px] text-stone-500">Username</span><span class="text-[13px] font-medium">{{ selectedParent.username }}</span></div>
            <div class="flex items-start justify-between"><span class="text-[12px] text-stone-500">Address</span><span class="text-[13px] font-medium flex items-center gap-1.5 text-right max-w-[220px]"><MapPin :size="13" class="text-stone-400 shrink-0" /> {{ selectedParent.address }}</span></div>
          </div>
          <div class="border-t border-stone-200 pt-5">
            <p class="text-[13px] font-semibold mb-3">Quick Statistics</p>
            <div class="grid grid-cols-3 gap-2">
              <div class="rounded-xl bg-stone-50 p-3 text-center"><p class="text-[18px] font-bold">{{ childrenOf(selectedParent).length }}</p><p class="text-[10.5px] text-stone-500 mt-0.5">Children</p></div>
              <div class="rounded-xl bg-sky-50 p-3 text-center"><p class="text-[18px] font-bold text-sky-700">{{ childrenOf(selectedParent).filter(x => x.child.vaccStatus === 'Upcoming').length }}</p><p class="text-[10.5px] text-stone-500 mt-0.5">Upcoming</p></div>
              <div class="rounded-xl bg-rose-50 p-3 text-center"><p class="text-[18px] font-bold text-rose-700">{{ childrenOf(selectedParent).filter(x => x.child.vaccStatus === 'Delayed').length }}</p><p class="text-[10.5px] text-stone-500 mt-0.5">Delayed</p></div>
            </div>
          </div>
          <div class="border-t border-stone-200 pt-5">
            <p class="text-[13px] font-semibold mb-3">Children Linked</p>
            <div class="space-y-2">
              <div v-for="x in childrenOf(selectedParent)" :key="x.rel.id" class="flex items-center gap-3 rounded-xl border border-stone-200 p-3">
                <div class="flex h-10 w-10 items-center justify-center rounded-xl bg-emerald-50 shrink-0"><Baby :size="17" class="text-emerald-700" /></div>
                <div class="flex-1 min-w-0"><p class="text-[12.5px] font-semibold truncate flex items-center gap-1.5">{{ x.child.name }} <Star v-if="x.rel.isPrimary" :size="11" class="text-amber-500 fill-amber-500" /></p><p class="text-[11px] text-stone-500">{{ x.child.id }} · {{ x.rel.relationship }} · {{ x.child.age }}</p></div>
                <div class="shrink-0"><VaccBadge :status="x.child.vaccStatus" /></div>
              </div>
              <p v-if="childrenOf(selectedParent).length === 0" class="text-[12.5px] text-stone-500">No children linked yet.</p>
            </div>
          </div>
          <div class="flex gap-2">
            <button @click="openRegister('child')" class="flex-1 flex items-center justify-center gap-1.5 rounded-xl border border-stone-200 px-3 py-2 text-[12.5px] font-medium hover:bg-stone-50"><Baby :size="14" /> Register Child</button>
            <button @click="openPicker('linkChild', { parent: selectedParent })" class="flex-1 flex items-center justify-center gap-1.5 rounded-xl border border-stone-200 px-3 py-2 text-[12.5px] font-medium hover:bg-stone-50"><Link2 :size="14" /> Link Existing Child</button>
          </div>
        </div>
      </aside>
    </transition>

    <!-- VIEW CHILD DRAWER -->
    <transition name="fade"><div v-if="showChildDrawer" class="fixed inset-0 z-30 bg-stone-900/30" @click="showChildDrawer = false"></div></transition>
    <transition name="slide">
      <aside v-if="showChildDrawer && selectedChild" class="fixed right-0 top-0 z-40 h-full w-[440px] bg-white border-l border-stone-200 shadow-xl overflow-y-auto">
        <div class="flex items-center justify-between px-5 py-4 border-b border-stone-200 sticky top-0 bg-white">
          <div><p class="text-[15px] font-semibold">Child Information</p><p class="text-[11.5px] text-stone-500">{{ selectedChild.id }}</p></div>
          <button @click="showChildDrawer = false" class="p-1.5 rounded-lg hover:bg-stone-100"><X :size="17" class="text-stone-500" /></button>
        </div>
        <div class="p-5 space-y-6">
          <div class="flex items-center gap-4">
            <div class="flex h-14 w-14 items-center justify-center rounded-2xl bg-emerald-50 shrink-0"><Baby :size="24" class="text-emerald-700" /></div>
            <div><p class="text-[16px] font-bold">{{ selectedChild.name }}</p><p class="text-[12px] text-stone-500">{{ selectedChild.age }} · {{ selectedChild.sex }}</p></div>
          </div>
          <div class="grid grid-cols-2 gap-3">
            <div class="rounded-xl bg-stone-50 p-3"><p class="text-[10.5px] uppercase tracking-wide text-stone-500">Birth Date</p><p class="text-[13px] font-medium mt-0.5">{{ selectedChild.birthDate }}</p></div>
            <div class="rounded-xl bg-stone-50 p-3"><p class="text-[10.5px] uppercase tracking-wide text-stone-500">Birth Place</p><p class="text-[13px] font-medium mt-0.5">{{ selectedChild.birthPlace }}</p></div>
            <div class="rounded-xl bg-stone-50 p-3"><p class="text-[10.5px] uppercase tracking-wide text-stone-500">Height</p><p class="text-[13px] font-medium mt-0.5">{{ selectedChild.height }}</p></div>
            <div class="rounded-xl bg-stone-50 p-3"><p class="text-[10.5px] uppercase tracking-wide text-stone-500">Weight</p><p class="text-[13px] font-medium mt-0.5">{{ selectedChild.weight }}</p></div>
          </div>
          <div class="border-t border-stone-200 pt-5">
            <p class="text-[13px] font-semibold mb-3">Vaccination Summary</p>
            <div class="rounded-xl bg-stone-50 p-3.5 space-y-2">
              <div class="flex items-center justify-between"><span class="text-[12px] text-stone-500">Status</span><VaccBadge :status="selectedChild.vaccStatus" /></div>
              <div class="flex items-center justify-between"><span class="text-[12px] text-stone-500">Next Vaccine</span><span class="text-[13px] font-medium">{{ selectedChild.nextVaccine }}</span></div>
            </div>
          </div>

          <!-- Vaccination History (incl. historical records entered before Aruga registration) -->
          <div class="border-t border-stone-200 pt-5">
            <div class="flex items-center justify-between mb-3">
              <p class="text-[13px] font-semibold">Vaccination History</p>
              <button @click="openHistoricalModal" class="flex items-center gap-1.5 rounded-lg bg-emerald-700 hover:bg-emerald-800 text-white px-2.5 py-1.5 text-[11.5px] font-semibold"><Syringe :size="13" /> Add Historical Record</button>
            </div>
            <p v-if="vaccinationLoading" class="text-[12px] text-stone-500 py-2">Loading vaccination records...</p>
            <div v-else-if="selectedChildVaccinations.length" class="rounded-xl border border-stone-200 overflow-hidden">
              <table class="w-full text-[12.5px]">
                <thead><tr class="bg-stone-50">
                  <th class="text-left font-semibold px-3 py-2 text-[10.5px] uppercase tracking-wide text-stone-500">Vaccine</th>
                  <th class="text-left font-semibold px-3 py-2 text-[10.5px] uppercase tracking-wide text-stone-500">Dose</th>
                  <th class="text-left font-semibold px-3 py-2 text-[10.5px] uppercase tracking-wide text-stone-500">Date</th>
                  <th class="text-left font-semibold px-3 py-2 text-[10.5px] uppercase tracking-wide text-stone-500">Status</th>
                </tr></thead>
                <tbody>
                  <tr v-for="v in selectedChildVaccinations" :key="v.vaccinationRecordID" class="border-t border-stone-200">
                    <td class="px-3 py-2">{{ v.vaccineName || vaccineNameById(v.vaccineID) }}</td>
                    <td class="px-3 py-2">{{ v.doseNumber }}</td>
                    <td class="px-3 py-2">{{ formatVaccDate(v.vaccinationDate) }}</td>
                    <td class="px-3 py-2"><span class="inline-flex items-center rounded-full px-2 py-0.5 text-[11px] font-medium bg-emerald-50 text-emerald-700">{{ v.status }}</span></td>
                  </tr>
                </tbody>
              </table>
            </div>
            <p v-else class="rounded-xl bg-stone-50 p-3.5 text-[12.5px] text-stone-500">No vaccination records yet.</p>
          </div>
          <!-- Parent / Guardian Accounts (the many-to-many bit) -->
          <div class="border-t border-stone-200 pt-5">
            <div class="flex items-center justify-between mb-1"><p class="text-[13px] font-semibold">Parent / Guardian Accounts</p><span class="text-[11px] text-stone-500">{{ linkedAccountsOf(selectedChild).length }} linked</span></div>
            <p class="text-[11.5px] text-stone-500 mb-3">Anyone linked here can view this child's vaccination record. Only one account can be the Primary Contact.</p>
            <div class="rounded-xl bg-stone-50 p-3 mb-3">
              <p class="text-[11px] font-medium text-stone-500 mb-2">Vaccination reminders notify</p>
              <div class="flex items-center rounded-lg border border-stone-200 p-1 bg-white w-fit">
                <button @click="selectedChild.notifyMode = 'primary'" class="rounded-md px-3 py-1.5 text-[12px] font-medium transition-colors" :class="selectedChild.notifyMode === 'primary' ? 'bg-emerald-700 text-white' : 'text-stone-500'">Primary Contact Only</button>
                <button @click="selectedChild.notifyMode = 'all'" class="rounded-md px-3 py-1.5 text-[12px] font-medium transition-colors" :class="selectedChild.notifyMode === 'all' ? 'bg-emerald-700 text-white' : 'text-stone-500'">All Linked Accounts</button>
              </div>
            </div>
            <div class="space-y-2">
              <div v-for="a in linkedAccountsOf(selectedChild)" :key="a.rel.id" class="rounded-xl border border-stone-200 p-3.5">
                <div class="flex items-start gap-3">
                  <Avatar :text="a.parent.initials" size="md" />
                  <div class="flex-1 min-w-0">
                    <p class="text-[13px] font-semibold flex items-center gap-1.5 flex-wrap">{{ a.parent.name }} <span v-if="a.rel.isPrimary" class="inline-flex items-center gap-1 text-[10.5px] font-medium text-amber-700 bg-amber-50 rounded-full px-2 py-0.5"><Star :size="10" class="fill-amber-600 text-amber-600" /> Primary Contact</span></p>
                    <p class="text-[12px] text-stone-500 mt-0.5">{{ a.rel.relationship }}</p>
                    <p class="text-[12px] text-stone-500 flex items-center gap-1.5 mt-1"><Phone :size="12" class="text-stone-400" /> {{ a.parent.contact }}</p>
                    <div class="mt-1.5"><StatusBadge :status="a.parent.status" /></div>
                  </div>
                </div>
                <div class="flex items-center gap-2 mt-3 pt-3 border-t border-stone-100">
                  <button @click="viewParent(a.parent)" class="flex items-center gap-1.5 rounded-lg border border-stone-200 px-2.5 py-1.5 text-[11.5px] font-medium hover:bg-stone-50"><Eye :size="13" /> View</button>
                  <button v-if="!a.rel.isPrimary" @click="setPrimary(selectedChild.id, a.rel.id)" class="flex items-center gap-1.5 rounded-lg border border-stone-200 px-2.5 py-1.5 text-[11.5px] font-medium hover:bg-stone-50"><ShieldCheck :size="13" /> Make Primary</button>
                  <button @click="openPicker('transfer', { rel: a.rel })" class="flex items-center gap-1.5 rounded-lg border border-stone-200 px-2.5 py-1.5 text-[11.5px] font-medium hover:bg-stone-50"><ArrowRightLeft :size="13" /> Transfer</button>
                  <div class="flex-1"></div>
                  <template v-if="removeConfirmId === a.rel.id">
                    <span class="text-[11px] text-rose-700 font-medium">Remove?</span>
                    <button @click="confirmRemoveAccount(a.rel)" class="rounded-lg bg-rose-600 text-white px-2.5 py-1.5 text-[11.5px] font-semibold hover:bg-rose-700">Yes</button>
                    <button @click="cancelRemoveAccount" class="rounded-lg border border-stone-200 px-2.5 py-1.5 text-[11.5px] font-medium hover:bg-stone-50">No</button>
                  </template>
                  <button v-else @click="askRemoveAccount(a.rel)" class="flex items-center gap-1.5 rounded-lg border border-stone-200 px-2.5 py-1.5 text-[11.5px] font-medium text-rose-700 hover:bg-rose-50"><Trash2 :size="13" /> Remove</button>
                </div>
              </div>
              <p v-if="linkedAccountsOf(selectedChild).length === 0" class="rounded-xl bg-rose-50 p-3.5 text-[12.5px] text-rose-700 flex items-center gap-2"><Link2 :size="14" /> No parent or guardian linked to this record yet.</p>
            </div>
            <button @click="openPicker('linkParent', { child: selectedChild })" class="mt-3 flex w-full items-center justify-center gap-1.5 rounded-xl border border-dashed border-stone-300 px-3 py-2.5 text-[12.5px] font-medium text-emerald-700 hover:bg-emerald-50"><Link2 :size="14" /> Link Another Parent / Guardian</button>
          </div>
        </div>
      </aside>
    </transition>

    <!-- GENERIC PICKER MODAL -->
    <div v-if="picker.open" class="fixed inset-0 z-50 flex items-center justify-center bg-stone-900/40 px-4">
      <div class="w-full max-w-lg rounded-2xl bg-white shadow-xl max-h-[85vh] flex flex-col">
        <div class="flex items-center justify-between px-6 py-4 border-b border-stone-200">
          <p class="text-[16px] font-semibold">{{ PICKER_CONFIG[picker.mode]?.title }}</p>
          <button @click="closePicker" class="p-1.5 rounded-lg hover:bg-stone-100"><X :size="18" class="text-stone-500" /></button>
        </div>
        <p v-if="picker.mode === 'transfer'" class="px-6 pt-4 text-[12.5px] text-stone-500">
          Replacing <span class="font-semibold text-stone-700">{{ parents.find(p => p.id === picker.rel?.parentId)?.name }}</span> ({{ picker.rel?.relationship }}) with a new guardian. The child's medical history is not affected.
        </p>
        <div class="p-6 pb-3">
          <div class="flex items-center gap-2 rounded-xl border border-stone-200 px-3 py-2.5">
            <Search :size="16" class="text-stone-400 shrink-0" />
            <input v-model="picker.query" :placeholder="PICKER_CONFIG[picker.mode]?.searchPlaceholder" class="flex-1 text-[13px] outline-none placeholder:text-stone-400" />
          </div>
        </div>
        <div class="flex-1 overflow-y-auto px-6 pb-2 space-y-2">
          <button v-for="item in pickerResults" :key="item.id" @click="picker.selected = item" class="flex w-full items-center gap-3 rounded-xl border p-3 text-left transition-colors" :class="picker.selected?.id === item.id ? 'border-emerald-500 bg-emerald-50' : 'border-stone-200 hover:bg-stone-50'">
            <div v-if="picker.mode === 'linkChild'" class="flex h-10 w-10 items-center justify-center rounded-xl bg-emerald-50 shrink-0"><Baby :size="17" class="text-emerald-700" /></div>
            <Avatar v-else :text="item.initials" size="md" />
            <div class="flex-1 min-w-0"><p class="text-[12.5px] font-semibold">{{ item.name }}</p><p class="text-[11px] text-stone-500">{{ item.id }} · <template v-if="picker.mode === 'linkChild'">Born {{ item.birthDate }}</template><template v-else>{{ item.contact }}</template></p></div>
            <Check v-if="picker.selected?.id === item.id" :size="17" class="text-emerald-700 shrink-0" />
          </button>
          <p v-if="pickerResults.length === 0" class="text-[12.5px] text-stone-500 py-4 text-center">No matches found.</p>
        </div>
        <div v-if="picker.selected" class="px-6 pb-4 space-y-3 border-t border-stone-200 pt-4">
          <div>
            <label class="text-[11.5px] font-medium text-stone-500">Relationship</label>
            <select v-model="picker.relationship" class="mt-1 w-full rounded-xl border border-stone-200 px-3 py-2.5 text-[13px] outline-none focus:border-emerald-500">
              <option value="" disabled>Choose relationship...</option>
              <option v-for="r in linkRelationshipOptions" :key="r">{{ r }}</option>
            </select>
          </div>
          <label v-if="PICKER_CONFIG[picker.mode]?.showPrimary" class="flex items-center gap-2 text-[12.5px] font-medium text-stone-700">
            <input type="checkbox" v-model="picker.isPrimary" class="rounded border-stone-300 text-emerald-600 focus:ring-emerald-500" /> Set as Primary Contact for this child
          </label>
        </div>
        <div v-if="picker.error" class="mx-6 mb-3 rounded-xl bg-rose-50 p-3 text-[12px] text-rose-700">
          {{ picker.error }}
        </div>
        <div class="flex items-center justify-end gap-3 px-6 py-4 border-t border-stone-200">
          <button :disabled="picker.submitting" @click="closePicker" class="rounded-xl px-4 py-2.5 text-[13px] font-medium text-stone-600 hover:bg-stone-50 disabled:opacity-40">Cancel</button>
          <button
            :disabled="!picker.selected || !picker.relationship || picker.submitting"
            @click="confirmPicker"
            class="rounded-xl px-5 py-2.5 text-[13px] font-semibold text-white bg-emerald-700 hover:bg-emerald-800 shadow-sm disabled:opacity-40 disabled:cursor-not-allowed"
          >
            {{ picker.submitting ? 'Saving...' : (picker.mode === 'transfer' ? 'Confirm Transfer' : 'Confirm Link') }}
          </button>
        </div>
      </div>
    </div>

    <!-- GENERIC REGISTER MODAL — this is "the creating account form" you asked about -->
    <div v-if="registerModal.open" class="fixed inset-0 z-50 flex items-center justify-center bg-stone-900/40 px-4">
      <div class="w-full max-w-xl rounded-2xl bg-white shadow-xl max-h-[88vh] overflow-y-auto">
        <div class="flex items-center justify-between px-6 py-4 border-b border-stone-200">
          <p class="text-[16px] font-semibold">Register {{ registerModal.kind === 'parent' ? 'Parent' : 'Child' }}</p>
          <button @click="closeRegister" class="p-1.5 rounded-lg hover:bg-stone-100"><X :size="18" class="text-stone-500" /></button>
        </div>
        <div v-if="registerModal.stage === 'form'" class="p-6 space-y-4">
          <p class="text-[12px] text-stone-500 -mt-1">This form only creates a {{ registerModal.kind }} record. {{ registerModal.kind === 'parent' ? 'Children can be registered or linked' : 'A parent can be created or linked' }} afterward.</p>
          <div class="grid grid-cols-2 gap-3">
            <template v-for="f in FORM_FIELDS[registerModal.kind]" :key="f.key">
              <select v-if="f.type === 'select'" v-model="registerModal.formData[f.key]" :class="f.full ? 'col-span-2' : ''" class="rounded-xl border border-stone-200 px-3 py-2.5 text-[13px] outline-none focus:border-emerald-500">
                <option value="" disabled selected>{{ f.placeholder || f.label }}</option>
                <option v-for="o in f.options" :key="o">{{ o }}</option>
              </select>
              <input v-else v-model="registerModal.formData[f.key]" :type="f.type === 'date' ? 'date' : (f.type === 'password' ? 'password' : 'text')" :placeholder="f.label" :class="f.full ? 'col-span-2' : ''" class="rounded-xl border border-stone-200 px-3 py-2.5 text-[13px] outline-none focus:border-emerald-500" />
            </template>
          </div>
          <div v-if="error" class="rounded-xl bg-rose-50 p-3 text-[12px] text-rose-700">{{ error }}</div>
        </div>
        <div v-else class="p-6 text-center py-10">
          <div class="flex h-14 w-14 items-center justify-center rounded-2xl bg-emerald-50 mx-auto mb-4"><Check :size="26" class="text-emerald-700" /></div>
          <p class="text-[15px] font-semibold mb-1">{{ registerModal.kind === 'parent' ? 'Parent' : 'Child' }} registered successfully.</p>
          <p class="text-[13px] text-stone-500 mb-5">Choose next action:</p>
          <div class="flex flex-col gap-2 max-w-xs mx-auto">
            <template v-if="registerModal.kind === 'parent'">
              <button @click="closeRegister(); openRegister('child')" class="rounded-xl px-4 py-2.5 text-[13px] font-semibold text-white bg-emerald-700 hover:bg-emerald-800">Register Child</button>
              <button @click="closeRegister(); openPicker('linkChild', { parent: selectedParent })" class="rounded-xl px-4 py-2.5 text-[13px] font-medium border border-stone-200 hover:bg-stone-50">Link Existing Child</button>
            </template>
            <template v-else>
              <button @click="closeRegister(); openRegister('parent')" class="rounded-xl px-4 py-2.5 text-[13px] font-semibold text-white bg-emerald-700 hover:bg-emerald-800">Create Parent Account</button>
              <button @click="closeRegister(); openPicker('linkParent', { child: selectedChild })" class="rounded-xl px-4 py-2.5 text-[13px] font-medium border border-stone-200 hover:bg-stone-50">Link Existing Parent</button>
            </template>
            <button @click="closeRegister" class="rounded-xl px-4 py-2.5 text-[13px] font-medium text-stone-500 hover:bg-stone-50">Finish</button>
          </div>
        </div>
        <div v-if="registerModal.stage === 'form'" class="flex items-center justify-end gap-3 px-6 py-4 border-t border-stone-200">
          <button @click="closeRegister" class="rounded-xl px-4 py-2.5 text-[13px] font-medium text-stone-600 hover:bg-stone-50">Cancel</button>
          <button @click="submitRegister" class="rounded-xl px-5 py-2.5 text-[13px] font-semibold text-white bg-emerald-700 hover:bg-emerald-800 shadow-sm">Register {{ registerModal.kind === 'parent' ? 'Parent' : 'Child' }}</button>
        </div>
      </div>
    </div>

    <!-- GENERIC EDIT MODAL -->
    <div v-if="editModal.open" class="fixed inset-0 z-50 flex items-center justify-center bg-stone-900/40 px-4">
      <div class="w-full max-w-lg rounded-2xl bg-white shadow-xl max-h-[88vh] overflow-y-auto">
        <div class="flex items-center justify-between px-6 py-4 border-b border-stone-200">
          <p class="text-[16px] font-semibold">Edit {{ editModal.kind === 'parent' ? 'Parent' : 'Child' }}</p>
          <button @click="editModal.open = false" class="p-1.5 rounded-lg hover:bg-stone-100"><X :size="18" class="text-stone-500" /></button>
        </div>
        <div class="p-6 space-y-4">
          <div v-for="row in fieldRows(EDIT_FIELDS[editModal.kind])" :key="row.map(f => f.key).join('-')" :class="row.length > 1 ? 'grid grid-cols-2 gap-3' : ''">
            <div v-for="f in row" :key="f.key">
              <label class="text-[11.5px] font-medium text-stone-500">{{ f.label }}</label>
              <select v-if="f.type === 'select'" v-model="editModal.item[f.key]" class="mt-1 w-full rounded-xl border border-stone-200 px-3 py-2.5 text-[13px] outline-none focus:border-emerald-500">
                <option v-for="o in f.options" :key="o">{{ o }}</option>
              </select>
              <input v-else v-model="editModal.item[f.key]" class="mt-1 w-full rounded-xl border border-stone-200 px-3 py-2.5 text-[13px] outline-none focus:border-emerald-500" />
            </div>
          </div>
        </div>
        <div class="flex items-center justify-end gap-3 px-6 py-4 border-t border-stone-200">
          <button @click="editModal.open = false" class="rounded-xl px-4 py-2.5 text-[13px] font-medium text-stone-600 hover:bg-stone-50">Cancel</button>
          <button @click="submitEdit" class="rounded-xl px-5 py-2.5 text-[13px] font-semibold text-white bg-emerald-700 hover:bg-emerald-800 shadow-sm">Save Changes</button>
        </div>
      </div>
    </div>
    <!-- HISTORICAL VACCINATION RECORDS MODAL -->
    <div v-if="historicalModal.open" class="fixed inset-0 z-50 flex items-center justify-center bg-stone-900/40 px-4">
      <div class="w-full max-w-xl rounded-2xl bg-white shadow-xl max-h-[88vh] overflow-y-auto">
        <div class="flex items-center justify-between px-6 py-4 border-b border-stone-200">
          <div>
            <p class="text-[16px] font-semibold">Historical Vaccination Records</p>
            <p class="text-[11.5px] text-stone-500 mt-0.5" v-if="selectedChild">{{ selectedChild.name }} · {{ selectedChild.id }}</p>
          </div>
          <button @click="closeHistoricalModal" class="p-1.5 rounded-lg hover:bg-stone-100"><X :size="18" class="text-stone-500" /></button>
        </div>

        <div v-if="!historicalModal.success" class="p-6 space-y-4">
          <p class="text-[12px] text-stone-500 -mt-1">Record vaccinations this child received before being managed in Aruga.</p>

          <div v-for="(row, idx) in historicalModal.rows" :key="idx" class="rounded-xl border border-stone-200 p-3.5 space-y-3">
            <div class="flex items-center justify-between">
              <p class="text-[11.5px] font-semibold text-stone-500">Vaccination {{ idx + 1 }}</p>
              <button v-if="historicalModal.rows.length > 1" @click="removeHistoricalRow(idx)" class="flex items-center gap-1 text-[11.5px] font-medium text-rose-700 hover:bg-rose-50 rounded-lg px-2 py-1"><Trash2 :size="12" /> Remove</button>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <select v-model="row.vaccineID" class="col-span-2 rounded-xl border border-stone-200 px-3 py-2.5 text-[13px] outline-none focus:border-emerald-500">
                <option value="" disabled>Select Vaccine</option>
                <option v-for="v in VACCINE_OPTIONS" :key="v.id" :value="v.id">{{ v.name }}</option>
              </select>
              <div>
                <label class="text-[11px] font-medium text-stone-500">Dose Number</label>
                <input v-model.number="row.doseNumber" type="number" min="1" class="mt-1 w-full rounded-xl border border-stone-200 px-3 py-2.5 text-[13px] outline-none focus:border-emerald-500" />
              </div>
              <div>
                <label class="text-[11px] font-medium text-stone-500">Vaccination Date</label>
                <input v-model="row.vaccinationDate" type="date" class="mt-1 w-full rounded-xl border border-stone-200 px-3 py-2.5 text-[13px] outline-none focus:border-emerald-500" />
              </div>
            </div>
          </div>

          <button @click="addHistoricalRow" class="flex w-full items-center justify-center gap-1.5 rounded-xl border border-dashed border-stone-300 px-3 py-2.5 text-[12.5px] font-medium text-emerald-700 hover:bg-emerald-50"><Syringe :size="14" /> Add Another Vaccination</button>

          <div v-if="historicalModal.error" class="rounded-xl bg-rose-50 p-3 text-[12px] text-rose-700">{{ historicalModal.error }}</div>
        </div>

        <div v-else class="p-6 text-center py-10">
          <div class="flex h-14 w-14 items-center justify-center rounded-2xl bg-emerald-50 mx-auto mb-4"><Check :size="26" class="text-emerald-700" /></div>
          <p class="text-[15px] font-semibold mb-1">Historical vaccination records saved successfully.</p>
          <p class="text-[13px] text-stone-500 mb-5">The child's vaccination history has been updated.</p>
          <button @click="closeHistoricalModal" class="rounded-xl px-5 py-2.5 text-[13px] font-semibold text-white bg-emerald-700 hover:bg-emerald-800">Done</button>
        </div>

        <div v-if="!historicalModal.success" class="flex items-center justify-end gap-3 px-6 py-4 border-t border-stone-200">
          <button @click="closeHistoricalModal" class="rounded-xl px-4 py-2.5 text-[13px] font-medium text-stone-600 hover:bg-stone-50">Cancel</button>
          <button :disabled="historicalModal.submitting" @click="submitHistoricalVaccinations" class="rounded-xl px-5 py-2.5 text-[13px] font-semibold text-white bg-emerald-700 hover:bg-emerald-800 shadow-sm disabled:opacity-50">
            {{ historicalModal.submitting ? "Saving..." : "Save Historical Vaccinations" }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
.slide-enter-active, .slide-leave-active { transition: transform 0.25s ease; }
.slide-enter-from, .slide-leave-to { transform: translateX(100%); }
</style>