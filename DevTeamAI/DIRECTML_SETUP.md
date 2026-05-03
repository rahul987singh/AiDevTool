# DirectML Model Setup

This project uses the DirectML Execution Provider natively to offload Matrix Multiplications (ONNX GenAI inference) to any DirectX 12 compatible GPU on Windows.

## Step 1: Download the DirectML Model

Download the Llama-3.1-8B-Instruct model with DirectML optimization for best GPU performance.

### Install Hugging Face CLI
```bash
pip install huggingface_hub
```

### Download the DirectML Weights

Run this script to download the model into the expected directory path:

```bash
python -c "
from huggingface_hub import snapshot_download
snapshot_download(
    repo_id='onnx-community/Meta-Llama-3.1-8B-Instruct-ONNX-DirectML-GenAI-INT4',
    local_dir='D:/Project/Models/Llama-3.1-8B-Instruct-ONNX'
)
print('Llama-3.1 model download complete!')
"
```

## Step 2: Validate the Directory Structure

Inside of `D:/Project/Models/Llama-3.1-8B-Instruct-ONNX/`, ensure these key files exist:
- `model.onnx`
- `model.onnx.data`
- `genai_config.json`
- `tokenizer.json`
- `vocab.json`

## Step 3: Configure Application

Update your `appsettings.json` file to point to the model location with GPU as primary and CPU fallback:

```json
{
  "LLM": {
    "BasePath": "D:/Project/Models/Llama-3.1-8B-Instruct-ONNX",
    "ModelId": "llama-3.1-8b-instruct",
    "ServiceId": "onnx-service-llama",
    "ExecutionProvider": "Auto"
  }
}
```

**Execution Provider Options:**
- `"Auto"`: Tries GPU first, falls back to CPU if unavailable
- `"DirectML"`: Forces GPU/DirectML execution only
- `"CPU"`: Forces CPU execution only

## Step 4: Run the Application

DirectML forces a strict **x64** architecture block. The app will automatically bootstrap the device using the `dml` Execution Provider.

```bash
dotnet run --project DevTeamAI\DevTeamAI.csproj
```

## Alternative: CPU-Only Setup

If you prefer CPU-only execution or need to force CPU usage:

```bash
python -c "
from huggingface_hub import snapshot_download
snapshot_download(
    repo_id='onnx-community/Meta-Llama-3.1-8B-Instruct-ONNX-DirectML-GenAI-INT4',
    local_dir='D:/Project/Models/Llama-3.1-8B-Instruct-ONNX'
)
print('CPU model download complete!')
"
```

Update `appsettings.json` for CPU execution:

```json
{
  "LLM": {
    "BasePath": "D:/Project/Models/Llama-3.1-8B-Instruct-ONNX",
    "ModelId": "llama-3.1-8b-instruct-cpu",
    "ServiceId": "onnx-service-llama-cpu",
    "ExecutionProvider": "CPU"
  }
}
```

## Required NuGet Packages

Add these packages to your DevTeamAI project:

```bash
dotnet add package Microsoft.SemanticKernel
dotnet add package Microsoft.SemanticKernel.Core
dotnet add package Microsoft.ML.OnnxRuntimeGenAI.DirectML
dotnet add package Microsoft.ML.OnnxRuntimeGenAI.Managed
```
