import { useState } from 'react'
import axios from 'axios'

export const useDataFetching = () => {
  const [candles, setCandles] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const fetchData = async (symbol, dateRange, interval) => {
    const fromDate = dateRange[0].startDate
    const toDate = dateRange[0].endDate
    
    if (!symbol || !fromDate || !toDate) {
      setError('Please fill all fields')
      return
    }

    setLoading(true)
    setError('')

    try {
      const response = await axios.get(`/api/price/${symbol}`, {
        params: {
          from: fromDate.toISOString(),
          to: toDate.toISOString(),
          interval: interval
        }
      })

      setCandles(response.data)
    } catch (err) {
      setError('Failed to fetch data: ' + err.message)
    } finally {
      setLoading(false)
    }
  }

  const clearError = () => setError('')

  return {
    candles,
    loading,
    error,
    fetchData,
    clearError,
  }
} 