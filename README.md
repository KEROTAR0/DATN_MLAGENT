Hướng Dẫn Cài Đặt ML-Agents cho Unity
# 1. Cài đặt ML-Agents trong Unity
Mở Unity.
Vào Window → Package Manager.
Nhấn + → Add package from git URL....
Nhập địa chỉ:
## https://github.com/Unity-Technologies/ml-agents.git?path=com.unity.ml-agents

Nhấn Add để cài đặt.

# 2. Cài đặt Python
Tải và cài đặt Python 3.10.0 từ python.org.
Khi cài đặt, nhớ chọn Add Python to PATH trước khi nhấn Install.

# 3. Kiểm tra Python đã cài đúng phiên bản
Mở Command Prompt (cmd) tại thư mục dự án Unity.
Gõ:
## py
Nếu hiện ra: Python 3.10.0 thì bạn đã cài đúng phiên bản.
Gõ:
## exit()
để thoát môi trường Python.

# 4. Tạo và kích hoạt môi trường ảo (virtual environment)
Tạo môi trường ảo:
## py -m venv venv
Kích hoạt môi trường ảo:
## venv\Scripts\activate
Khi thành công, bạn sẽ thấy dòng lệnh chuyển thành:

### (venv) C:\...

# 5. Cập nhật pip
Trong môi trường ảo:
## py -m pip install --upgrade pip

6. Cài đặt ML-Agents
Cài đặt ML-Agents:
## pip install mlagents

# 7. (Khuyến nghị) Cài đặt thủ công Torch và Numpy tương thích
Mặc dù mlagents sẽ tự cài, nhưng cài thủ công giúp bạn chủ động kiểm soát phiên bản và tránh lỗi.
Cài Torch bản CPU:
## pip install torch==2.0.1
Cập nhật Numpy:
## pip install numpy --upgrade

# 8. (Tuỳ chọn) Cài Torch hỗ trợ GPU CUDA (nếu máy bạn có GPU NVIDIA)
Truy cập: https://pytorch.org/get-started/locally/

Chọn:
PyTorch Build: Stable
Your OS: Windows
Package: pip
Language: Python
Compute Platform: CUDA 11.8 (hoặc phiên bản phù hợp với GPU của bạn)

Sau đó copy lệnh phù hợp, ví dụ:
## pip install torch==2.0.1+cu118 torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118
Lưu ý: Nếu máy bạn không có GPU hoặc không biết rõ, hãy cài bản CPU mặc định.

# 9. Kiểm tra cài đặt thành công
Kiểm tra phiên bản torch:
## python -c "import torch; print(torch.__version__)"
Kiểm tra ML-Agents:
## mlagents-learn --help
Nếu các lệnh trên chạy không lỗi, nghĩa là bạn đã cài đặt thành công!

# 10. Ghi chú thêm

Luôn đảm bảo bạn đang trong môi trường ảo ((venv)).

Nếu Unity báo lỗi khi training, kiểm tra kỹ lại phiên bản Python, Torch và ML-Agents.

Nếu cần huỷ môi trường ảo, gõ:
## deactivate
