import { useEffect, useRef } from 'react'
import { createChart } from 'lightweight-charts'

const CHART_CONFIG = {
  layout: {
    background: { type: 'solid', color: 'transparent' },
    textColor: '#e0e6ed',
    fontSize: 12,
    fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
  },
  grid: {
    vertLines: { 
      color: 'rgba(255, 255, 255, 0.05)',
      style: 1,
      visible: true,
    },
    horzLines: { 
      color: 'rgba(255, 255, 255, 0.05)',
      style: 1,
      visible: true,
    },
  },
  crosshair: {
    mode: 1,
    vertLine: {
      color: '#00d4aa',
      width: 1,
      style: 2,
      labelBackgroundColor: '#00d4aa',
    },
    horzLine: {
      color: '#00d4aa',
      width: 1,
      style: 2,
      labelBackgroundColor: '#00d4aa',
    },
  },
  rightPriceScale: {
    borderColor: 'rgba(255, 255, 255, 0.1)',
    textColor: '#e0e6ed',
    fontSize: 11,
    scaleMargins: { top: 0.1, bottom: 0.1 },
  },
  timeScale: {
    borderColor: 'rgba(255, 255, 255, 0.1)',
    textColor: '#e0e6ed',
    fontSize: 11,
    timeVisible: true,
    secondsVisible: false,
    rightOffset: 12,
    barSpacing: 8,
    minBarSpacing: 3,
    tickMarkFormatter: (time) => {
      const date = new Date(time * 1000)
      return date.toLocaleDateString('en-US', { 
        month: 'short', 
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })
    },
  },
  width: 0,
  height: 500,
  handleScroll: {
    mouseWheel: true,
    pressedMouseMove: true,
    horzTouchDrag: true,
    vertTouchDrag: true,
  },
  handleScale: {
    axisPressedMouseMove: { time: true, price: true },
    axisDoubleClick: { time: true, price: true },
    mouseWheel: true,
    pinch: true,
  },
}

const SERIES_CONFIG = {
  upColor: '#00d4aa',
  downColor: '#ff4757',
  borderDownColor: '#ff4757',
  borderUpColor: '#00d4aa',
  wickDownColor: '#ff4757',
  wickUpColor: '#00d4aa',
  priceFormat: {
    type: 'price',
    precision: 2,
    minMove: 0.01,
  },
}

function CandlestickChart({ candles, symbol }) {
  const chartContainerRef = useRef()
  const chart = useRef()
  const candlestickSeries = useRef()

  useEffect(() => {
    if (!chartContainerRef.current) return

    try {
      const config = {
        ...CHART_CONFIG,
        width: chartContainerRef.current.clientWidth,
        watermark: {
          visible: true,
          fontSize: 24,
          horzAlign: 'center',
          vertAlign: 'center',
          color: 'rgba(0, 212, 170, 0.1)',
          text: symbol,
          fontFamily: 'Inter, sans-serif',
          fontStyle: 'bold',
        },
      }

      chart.current = createChart(chartContainerRef.current, config)
      candlestickSeries.current = chart.current.addCandlestickSeries(SERIES_CONFIG)
    } catch (error) {
      console.error('Error creating chart:', error)
    }

    const handleResize = () => {
      if (chart.current && chartContainerRef.current) {
        chart.current.applyOptions({ 
          width: chartContainerRef.current.clientWidth
        })
      }
    }

    window.addEventListener('resize', handleResize)

    return () => {
      window.removeEventListener('resize', handleResize)
      if (chart.current) {
        chart.current.remove()
      }
    }
  }, [symbol])

  useEffect(() => {
    if (!candlestickSeries.current || !candles.length) return

    try {
      const candleData = candles.map(candle => ({
        time: Math.floor(new Date(candle.openTime).getTime() / 1000),
        open: parseFloat(candle.open),
        high: parseFloat(candle.high),
        low: parseFloat(candle.low),
        close: parseFloat(candle.close)
      }))

      candlestickSeries.current.setData(candleData)

      // Настройка отображения с небольшими отступами
      const timeScale = chart.current.timeScale()
      const firstTime = candleData[0].time
      const lastTime = candleData[candleData.length - 1].time
      const totalTimeRange = lastTime - firstTime
      const padding = totalTimeRange * 0.05 // 5% отступы

      timeScale.setVisibleRange({
        from: firstTime - padding,
        to: lastTime + padding
      })
    } catch (error) {
      console.error('Error setting candle data:', error)
    }
  }, [candles])

  return (
    <div 
      ref={chartContainerRef} 
      style={{ 
        width: '100%', 
        height: '500px',
        borderRadius: '12px',
        overflow: 'hidden',
        position: 'relative',
        background: 'transparent',
      }} 
    />
  )
}

export default CandlestickChart 