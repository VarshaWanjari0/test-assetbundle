# Unity AssetBundle Builder

This repository contains the extracted Unity project (`Test`) and a GitHub Actions workflow to build Unity AssetBundles.

## Unity Project Details
- **Unity Version:** `2021.3.43f1`
- **Project Directory:** `Test/`
- **AssetBundle Assets:**
  - Asset name: `rgs` (located in `Test/Assets/outsidemods/prefab/rgs.prefab`)

## GitHub Actions Workflow
The workflow `.github/workflows/build-assetbundle.yml` automates building the AssetBundles using GameCI.

### Triggering a Build
1. Go to the **Actions** tab in GitHub.
2. Select **Build AssetBundle**.
3. Click **Run workflow**.
4. Choose your target platform:
   - `Android` (Default)
   - `StandaloneWindows64`
   - `StandaloneLinux64`
   - `iOS`
5. Click **Run workflow**.

Once complete, the built AssetBundle artifacts will be uploaded and downloadable directly from the GitHub Actions run summary.

## Unity License
The Unity license file (`Unity_lic.ulf`) is included in the repository for automated builds. You can also configure the `UNITY_LICENSE` repository secret in GitHub Settings if preferred.
