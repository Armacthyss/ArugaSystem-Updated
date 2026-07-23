# QUICK START GUIDE - Testing the Implementation

## Prerequisites
- ✅ .NET 8.0 SDK installed
- ✅ SQL Server with ArugaSystemDB database
- ✅ Node.js/npm installed
- ✅ Both backend and frontend on same network (localhost)

---

## Step 1: Build & Run Backend

```bash
# Navigate to backend directory
cd "C:\Users\renzo\Desktop\ARUGA_Capstone\AndroidWebAPI\AndroidWebAPI"

# Restore NuGet packages (installs BCrypt.Net-Core)
dotnet restore

# Build the project
dotnet build

# Run the backend
dotnet run
```

**Expected Output:**
```
Building...
Built successfully.
info: Program[0]
      Listening on http://localhost:57147
```

**Verify:**
- Backend running on http://localhost:57147
- Swagger docs at http://localhost:57147/swagger/index.html
- Can see new endpoints in Swagger:
  - POST /api/Parents (Create Parent)
  - GET /api/Parents/all (Get All Parents)
  - GET /api/Parents/{id} (Get Parent)
  - PUT /api/Parents/{id} (Update Parent)
  - POST /api/Children (Create Child)
  - PUT /api/Children/{id} (Update Child)

---

## Step 2: Run Frontend

```bash
# Navigate to frontend directory
cd "C:\Users\renzo\Desktop\ARUGA_Capstone\ArugaSystem"

# Optional: Install dependencies if not already done
npm install

# Run development server
npm run dev
```

**Expected Output:**
```
VITE v8.0.8  ready in 234 ms
➜  Local:   http://localhost:5174/
```

---

## Step 3: Test Parent Registration

### Via UI (Easiest):
1. Open browser: http://localhost:5174/staff/patient-records
2. Click blue **"+ Register Parent"** button
3. Fill form:
   - Full Name: `John Smith`
   - Contact Number: `09123456789`
   - Email: `john.smith@test.com`
   - Password: `TestPass123`
   - Address: `123 Main St`
   - Barangay: `Barangay 1`
4. Click **"Register Parent"** button
5. Should see success message
6. Parent appears in "Parents" tab with "0 parents" updated

### Via Swagger API:
1. Go to http://localhost:57147/swagger/index.html
2. Click **POST /api/Parents** (Create)
3. Click "Try it out"
4. Enter JSON:
```json
{
  "firstName": "Maria",
  "lastName": "Gonzales",
  "middleName": "Cruz",
  "email": "maria.gonzales@test.com",
  "contactNo": "09876543210",
  "barangayNo": "Barangay 2",
  "address": "456 Oak Ave",
  "password": "SecurePass456"
}
```
5. Click **"Execute"**
6. Should see 201 Created response with ParentID

---

## Step 4: Test Child Registration

### Via UI:
1. Still on http://localhost:5174/staff/patient-records
2. Click blue **"+ Register Child"** button
3. Select parent from dropdown (should show the one we just created)
4. Fill form:
   - Full Name: `Alex Smith`
   - Birth Date: `2022-06-15`
   - Sex: `Male`
   - Address: `123 Main St`
   - Health Center: `City Health Center`
5. Click **"Register Child"** button
6. Should see success message
7. Child appears in "Children" tab

### Via Swagger API:
1. Go to http://localhost:57147/swagger/index.html
2. Click **POST /api/Children** (Create)
3. Click "Try it out"
4. Enter JSON:
```json
{
  "firstName": "Luis",
  "lastName": "Gonzales",
  "middleName": "Maria",
  "birthDate": "2023-03-20T00:00:00",
  "placeOfBirth": "City Hospital",
  "sex": "Male",
  "barangay": 2,
  "birthHeight": 50.5,
  "birthWeight": 3.2,
  "address": "456 Oak Ave",
  "healthCenter": "Barangay Health Station",
  "parentID": "<paste-parent-id-from-previous-response>",
  "motherID": null,
  "fatherID": null,
  "guardianID": null,
  "motherName": "Diana Gonzales",
  "fatherName": "Pedro Gonzales",
  "guardianName": null
}
```
5. Click **"Execute"**
6. Should see 201 Created response with ChildID

---

## Step 5: Test Data Retrieval

### Get All Parents:
```bash
curl http://localhost:57147/api/Parents/all
```

Expected response:
```json
[
  {
    "parentID": "550e8400-e29b-41d4-a716-446655440000",
    "firstName": "John",
    "middleName": "",
    "lastName": "Smith",
    "email": "john.smith@test.com",
    "contactNo": "09123456789",
    "barangayNo": "Barangay 1",
    "address": "123 Main St"
  }
]
```

### Get All Children:
```bash
curl http://localhost:57147/api/Children/all
```

### Get Children by Parent:
```bash
curl http://localhost:57147/api/Children/parent/{parentID}
```

---

## Step 6: Test Login with New Account

1. Open http://localhost:5174/ParentLogin (or navigate from main page)
2. Login with:
   - Email: `john.smith@test.com`
   - Password: `TestPass123`
3. Should see parent dashboard
4. Should see registered child(ren)

---

## Troubleshooting

### "Failed to load data from server"
**Cause:** Backend not running or CORS issue
**Fix:**
- Check backend is running on port 57147
- Check firewall isn't blocking localhost
- Look in browser DevTools Console (F12) for actual error
- Check browser Network tab for API response

### Database errors
**Cause:** Connection string or database issues
**Fix:**
- Verify appsettings.json has correct connection string
- Check SQL Server is running
- Verify ArugaSystemDB database exists
- Run migrations if needed: `dotnet ef database update`

### Port already in use
**Cause:** Another app using port 57147 or 5174
**Fix:**
```bash
# Find process using port 57147
netstat -ano | findstr :57147

# Kill process (replace PID)
taskkill /PID <PID> /F
```

### "Email is already registered"
**Cause:** Trying to register with email that exists
**Fix:**
- Use different email
- Delete existing record from database if it's test data

---

## Success Indicators ✅

After completing all steps, you should have:

✅ Parent account created in database with hashed password
✅ Child record created and linked to parent
✅ Data visible in UI without errors
✅ Parent can login with new account
✅ All API endpoints responding correctly
✅ No console errors in browser
✅ No errors in backend logs

---

## Database Verification (Optional)

Connect to SQL Server and run:

```sql
-- Check parents were created
SELECT ParentID, FirstName, LastName, Email 
FROM dbo.Parents 
WHERE Email LIKE 'john.smith@test.com';

-- Check children were created
SELECT ChildID, FirstName, LastName, BirthDate, ParentID 
FROM dbo.Children 
WHERE ParentID IN (
  SELECT ParentID FROM dbo.Parents 
  WHERE Email LIKE 'john.smith@test.com'
);
```

---

## Next Testing Steps

1. **Edit Registration:** Click edit icon on parent/child, modify fields
2. **Delete Operations:** Test removing records (not yet implemented but models support it)
3. **Link Family Members:** Add mother/father/guardian relationships
4. **Vaccination Flow:** Link vaccinations to registered children
5. **Notifications:** Verify notifications trigger for upcoming vaccinations
6. **Stress Test:** Register multiple parents and children, verify performance

---

**Questions?** Check the IMPLEMENTATION_SUMMARY.md for detailed technical documentation.
