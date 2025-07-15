import { useState } from 'react'
import { getPriceApi } from '../services/apiProvider'

export const useDataFetching = () => {
  const [candles, setCandles] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const priceApi = getPriceApi();

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
      const responseApi = await priceApi.apiPriceSymbolGet(
        symbol,
        fromDate.toISOString(),
        toDate.toISOString(),
        interval);

      setCandles(responseApi.data)
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