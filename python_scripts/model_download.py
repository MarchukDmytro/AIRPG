
import os
import subprocess
from huggingface_hub import hf_hub_download

# === CONFIG ===
MODEL_NAME = "qwen3-heretic-Q6_K_L"
GGUF_HF_REPO = "bartowski/p-e-w_Qwen3-4B-Instruct-2507-heretic-GGUF"
GGUF_FILE = "p-e-w_Qwen3-4B-Instruct-2507-heretic-Q6_K_L.gguf"
LOCAL_DIR = "./models"
MODFILE_PATH = os.path.join(LOCAL_DIR, "Modelfile")

# Ensure local directory exists
os.makedirs(LOCAL_DIR, exist_ok=True)
GGUF_PATH = os.path.join(LOCAL_DIR, GGUF_FILE)

# === 1️⃣ Download GGUF if missing ===
if not os.path.exists(GGUF_PATH):
    print(f"[+] GGUF not found, downloading {GGUF_FILE}...")
    hf_hub_download(
        repo_id=GGUF_HF_REPO,
        filename=GGUF_FILE,
        local_dir=LOCAL_DIR,
        force_download=False
    )
else:
    print(f"[+] GGUF already exists at {GGUF_PATH}")

# === 2️⃣ Create Modelfile if missing ===
if not os.path.exists(MODFILE_PATH):
    print("[+] Creating Modelfile...")
    modelfile_content = f"""
FROM ./{GGUF_FILE}

TEMPLATE \"\"\"{{{{ if .System }}}}<|im_start|>system<|im_sep|>{{{{ .System }}}}<|im_end|>{{{{ end }}}}{{{{ if .Prompt }}}}<|im_start|>user<|im_sep|>{{{{ .Prompt }}}}<|im_end|>{{{{ end }}}}<|im_start|>assistant<|im_sep|>{{{{ .Response }}}}<|im_end|>\"\"\"

PARAMETER stop "<|im_start|>"
PARAMETER stop "<|im_end|>"
PARAMETER temperature 0.7
PARAMETER top_p 0.9
PARAMETER num_ctx 8192
""".strip()
    with open(MODFILE_PATH, "w") as f:
        f.write(modelfile_content)
else:
    print("[+] Modelfile already exists")

# === 3️⃣ Create Ollama model ===
print("[+] Building Ollama model (this may take a while)...")
subprocess.run(
    ["ollama", "create", MODEL_NAME, "-f", MODFILE_PATH],
    check=True
)

print(f"[+] Done! You can now run: ollama run {MODEL_NAME}")
