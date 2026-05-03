import os
from huggingface_hub import snapshot_download

# Configuration
repo_id = "onnx-community/Meta-Llama-3.1-8B-Instruct-ONNX-DirectML-GenAI-INT4"
local_dir = "D:/Project/Models/Llama-3.1-8B-Instruct-ONNX"

def download_model():
    print(f"Starting download of {repo_id}...")
    if not os.path.exists(local_dir):
        os.makedirs(local_dir)
        
    try:
        # Download the model from Hugging Face
        snapshot_download(
            repo_id=repo_id,
            local_dir=local_dir,
            local_dir_use_symlinks=False,
            revision="main"
        )
        print(f"Model downloaded successfully to {local_dir}")
    except Exception as e:
        print(f"Error downloading model: {e}")

if __name__ == "__main__":
    download_model()
