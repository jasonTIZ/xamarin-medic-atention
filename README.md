# Medical Attention

Rural medical attention system: **ASP.NET Core API** backend and **Xamarin.Forms** mobile app.

## Install the app on your phone (without building on Linux)

The mobile front is **Xamarin** and is built on **GitHub Actions** (Windows). On your machine you only install the APK and run the API.

> Spanish version: [Medical-atentionApp/INSTALAR-EN-TELEFONO.md](Medical-atentionApp/INSTALAR-EN-TELEFONO.md)

### 1. Trigger the build on GitHub

1. Push this repo to **GitHub** (if it is not there yet).
2. Open **Actions** → workflow **Build Android APK**.
3. On **push to any branch** (when something under `Medical-atentionApp/` changes), or click **Run workflow** (manual).

When the run succeeds, open it → **Artifacts** → download `medical-atention-android-debug` (ZIP containing the `.apk`).

### 2. Run the API on your PC (Linux)

```bash
cd MedicalAtention.API
dotnet run --urls "http://0.0.0.0:5258"
```

Note your machine IP:

```bash
hostname -I | awk '{print $1}'
```

In the team codebase, set `AppConstants.ApiBaseUrl` to `http://YOUR_IP:5258` (phone and PC on the **same Wi‑Fi**).

Path: `Medical-atentionApp/Medical-atention/Medical-atention/Constants/AppConstants.cs`

### 3. Install on your phone

Enable USB debugging and connect the device:

```bash
adb devices
adb install -r ~/Downloads/com.companyname.medical_atention-Signed.apk
```

Adjust the path to the `.apk` you extracted from the artifact.

### 4. Test login

| Email | Password |
|-------|----------|
| `admin@medic.com` | `Admin123!` |
| `doctor@medic.com` | `Doctor123!` |

### If the workflow fails

- Check the log under **Actions** (first run may take longer while Xamarin is installed on the runner).
- Run again with **Run workflow**.
- Share the specific error from the log with the team.

### Quick reference

| Step | Where |
|------|--------|
| Build APK | GitHub Actions (automatic) |
| API | Your PC: `dotnet run` |
| Install app | Your PC: `adb install -r` |

## Project layout

| Path | Description |
|------|-------------|
| `MedicalAtention.API/` | REST API (.NET 9, SQLite) |
| `Medical-atentionApp/` | Xamarin.Forms solution (Android / iOS) |

## Run the API locally

```bash
cd MedicalAtention.API
dotnet run
```

Default URL: `http://localhost:5258`
