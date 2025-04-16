# HURO Unity VR Environment
**[HURO APK](https://drive.google.com/file/d/1tVfMlSEDXV_SN2XM_-t7uihdwUckJqEs/view?usp=sharing)**


## Setup

1. Download [Firebase SDK](https://firebase.google.com/download/unity?hl=en&authuser=0&_gl=1*1hkvyve*_ga*MjczMTY1NDA4LjE3MzcyNTY4NDI.*_ga_CW55HF8NVT*MTczODE4NDEzMS41LjEuMTczODE4Nzk3Ni42LjAuMA..)

2. In your open Unity project, navigate to Assets > Import Package > Custom Package.

3. From the unzipped SDK, select to import the SDK for Firebase Storage.

4. In the Import Unity Package window, click Import.

5. Delete `Assets/ExternalDependencyManager/Editor/Google.IOSResolver.dll` and `Assets/ExternalDependencyManager/Editor/Google.IOSResolver.dll.mdb`

6. Locate `Assets/Firebase/Editor/Firebase.Editor.dll.meta` and uncheck `Validate References` in the inspector.

7. Install `iOS Build Support` in editor modules.
8. Add firebase google-services.json to `HURO_VR/Assets`


## Documentation

### MR scene

_Scene:_ MixedReality

*Running:* 

1. Choose the MR room you'd like to test under `MRUK` gameObject > `Scene Settings` > `Room Prefabs` > `Room Index`.
This will choose `Room Index` room from `Room Prefabs`. You can find more rooms in `Resources/Training_Environments/Prefabs`. These rooms are designed to simulate a room scan (Meta Scene recognition).

2. Press play. Currently Robot and Goal are placed randomly in the scene. Ideally we'd have a minimum distance they must be apart by.

3. Initialize algorithm by pressing the `I` key. This will label the scene. Go to `Scene View` to see drawn gizmos that represent the algorithm's view of the scene.

4. Press `Space` to start running the algorithm.


### Storage Class

_Firebase Storage + Firestore handler_
##### Public Functions:
`DownloadFile("filename", FileType, OnDownloaded);`

`filename` - Name of file in database, including extension (ex: .txt or .py)

`FileType` - Type of file to be retrieved. (ex: algorithm or model)

`OnDownloaded` - Callback that will be passed the data as a `byte[]`. Function must convert data to desired type.

Example Usage:
```
void Example() {
    FindAnyObjectByType<Storage>().DownloadFile("sample.txt", FileType.Algorithm, (bytes) =>
    {
        Debug.Log(Encoding.UTF8.GetString(bytes));
    });
}

```

**Note**: Ensure `FirebaseStorageInstance` has been added to the scene.


### ModelTester Class
##### Public Members:
`Database_Models.SimulationMetaData[] simMetaDatas` - List of type `Database_Models.SimulationMetaData` that stores all simulation bundles for user's organization.

`bool recievedMetaData` - True if simMetaDatas has finished loading.

##### Public Functions:
`void RunSimulation()` - Runs the selected simulation bundle. See `SelectSimulation` for selection.

`void SelectSimulation(Database_Models.SimulationMetaData bundle)` - Selects `bundle` as the simulation to run.

### Running Python In Unity
[Example](https://mikalikes.men/use-python-with-unity-3d-the-definitive-guide/)

### Access GCP Cloud Compute

*Access VM:* `gcloud compute ssh --zone "us-central1-c" "huro-compute-engine" --project "canvas-spark-452121-q7"`

*Copy files from local machine to VM*: `gcloud compute scp <path_to_local_file> huro-compute-engine:/<path_in_vm> --zone "us-central1-c"`

Use flag `--recurse` to upload folders.
