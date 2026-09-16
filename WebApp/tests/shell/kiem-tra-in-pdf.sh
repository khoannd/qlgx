#!/usr/bin/env bash
# Kiem thu khoi: image Docker co in duoc PDF khong.
#
# Vi sao phai kiem o TANG IMAGE chu khong phai bang test .NET thong thuong: loi o day khong
# phai loi logic ma la loi THIEU THU VIEN HE THONG trong image chay. Bo test .NET chay tren may
# phat trien (Windows, co san Chromium cua Playwright) se luon xanh du image hoan toan hong.
set -euo pipefail

readonly ANH="${1:-qlgx-api:kiem-thu}"

echo "==> Kiem tra Chromium co trong image $ANH khong"
docker run --rm --entrypoint sh "$ANH" -c '
  set -e
  test -n "$PLAYWRIGHT_BROWSERS_PATH" || { echo "THIEU bien PLAYWRIGHT_BROWSERS_PATH"; exit 1; }
  duong_dan=$(find "$PLAYWRIGHT_BROWSERS_PATH" -name headless_shell -o -name chrome | head -1)
  test -n "$duong_dan" || { echo "KHONG tim thay Chromium trong $PLAYWRIGHT_BROWSERS_PATH"; exit 1; }
  echo "Tim thay: $duong_dan"
  "$duong_dan" --headless --no-sandbox --version
'
echo "==> DAT: image co Chromium chay duoc"

# Khong dung lai o "chay duoc --version": Chromium co the khoi dong nhung van khong sinh ra
# duoc PDF (thieu font, thieu thu vien render, sandbox bi chan trong container...). Buoc duoi
# day mo phong dung viec BoTrinhDuyet.cs lam that: in mot trang HTML ra PDF trong bo nho, roi
# kiem 5 byte dau la chu ky PDF "%PDF-". Khong can co so du lieu — day chinh la che do hong ma
# task nay nham vao (thieu thu vien he thong cua Chromium), khong phai loi logic nghiep vu.
echo "==> Kiem tra Chromium co in ra PDF hop le khong"
KET_QUA=$(docker run --rm --entrypoint sh "$ANH" -c '
  set -e
  bin=$(find "$PLAYWRIGHT_BROWSERS_PATH" \( -name headless_shell -o -name chrome \) | head -1)
  echo "<h1>Thu in tieng Viet co dau</h1>" > /tmp/thu.html
  "$bin" --headless --no-sandbox --disable-gpu --print-to-pdf=/tmp/thu.pdf /tmp/thu.html >/tmp/log 2>&1
  head -c 5 /tmp/thu.pdf
')
echo "Dau tep PDF: $KET_QUA"
[ "$KET_QUA" = "%PDF-" ] || { echo "THAT BAI: Chromium khong sinh ra PDF hop le (dau tep khong phai %PDF-)"; exit 1; }
echo "==> DAT: Chromium in ra PDF hop le (%PDF-)"
