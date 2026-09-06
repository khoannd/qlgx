// ag-grid đo kích thước vùng chứa (qua ResizeObserver và getBoundingClientRect) để quyết
// định vẽ cột/dòng nào. jsdom không có layout engine: không có ResizeObserver và mọi phần
// tử đều rộng/cao bằng 0, nên lưới không vẽ được gì trong môi trường test nếu thiếu phần
// giả lập dưới đây.
class ResizeObserverGia {
  observe() {}
  unobserve() {}
  disconnect() {}
}

if (!('ResizeObserver' in globalThis)) {
  ;(globalThis as unknown as { ResizeObserver: typeof ResizeObserverGia }).ResizeObserver =
    ResizeObserverGia
}

Object.defineProperty(HTMLElement.prototype, 'offsetWidth', { configurable: true, value: 1000 })
Object.defineProperty(HTMLElement.prototype, 'offsetHeight', { configurable: true, value: 600 })
Object.defineProperty(HTMLElement.prototype, 'clientWidth', { configurable: true, value: 1000 })
Object.defineProperty(HTMLElement.prototype, 'clientHeight', { configurable: true, value: 600 })

HTMLElement.prototype.getBoundingClientRect = function getBoundingClientRect() {
  return {
    x: 0, y: 0, width: 1000, height: 600, top: 0, left: 0, right: 1000, bottom: 600,
    toJSON() { return this },
  } as DOMRect
}
