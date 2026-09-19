# Tu Tiên Du Hành MVP

Vertical slice của game nhập vai 2D chạy trực tiếp trên trình duyệt, ưu tiên thiết bị cấu hình thấp và phiên chơi ngắt quãng.

## Chơi ngay trên máy

Không cần cài package hay build. Do PWA service worker cần HTTP, hãy chạy một static server bất kỳ:

```bash
cd game
python -m http.server 8080
```

Mở `http://localhost:8080`.

## Tính năng đã có

- Bản đồ 12 x 8, điều khiển bằng WASD, phím mũi tên hoặc nút cảm ứng.
- Quái thường, tinh anh, quái hiếm và chiến đấu bán tự động.
- Rương cơ duyên sinh ngẫu nhiên, ba cấp phần thưởng.
- Mười nhóm nghề nghiệp đời thực với bonus riêng.
- Trang bị, túi đồ và thuộc tính cộng thêm theo nghề.
- Năm công pháp mẫu, gồm công pháp vượt cấp và công pháp khóa cảnh giới.
- Tu luyện theo thời gian, tiếp tục khi đóng trình duyệt.
- Tin đồn cơ duyên không tiết lộ vị trí chính xác.
- Màn hình thử hệ số quái, rương, quái hiếm và linh khí.
- Lưu localStorage, PWA cache và giao diện responsive.

## Giới hạn của vertical slice

Đây là prototype gameplay chạy hoàn toàn phía trình duyệt. Chưa có tài khoản, máy chủ có thẩm quyền, SQL Server, GPS thật, song tu nhiều người hoặc đồng bộ nhiều thiết bị. Các phần đó thuộc giai đoạn MVP online tiếp theo; không nên dùng logic ngẫu nhiên phía client cho bản production.

## Cấu trúc

```text
game/
  index.html
  styles.css
  app.js
  manifest.webmanifest
  sw.js
  icon.svg
```

## Kiểm thử nhanh

1. Tạo nhân vật và chọn nghề.
2. Di chuyển đến quái hoặc rương trên bản đồ.
3. Chiến đấu, nhận đồ và trang bị.
4. Học rồi vận hành công pháp.
5. Bắt đầu tu luyện, đóng tab và quay lại khi hết thời gian.
6. Thay đổi tỷ lệ trong Thiết lập MVP và sinh lại bản đồ.
