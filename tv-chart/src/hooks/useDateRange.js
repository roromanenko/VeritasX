import { useState, useEffect, useRef } from 'react'
import { addDays, format } from 'date-fns'

export const useDateRange = () => {
  const [dateRange, setDateRange] = useState([
    {
      startDate: new Date(),
      endDate: addDays(new Date(), 7),
      key: 'selection',
    },
  ])
  const [showCalendar, setShowCalendar] = useState(false)
  const calendarRef = useRef(null)

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (calendarRef.current && !calendarRef.current.contains(event.target)) {
        setShowCalendar(false)
      }
    }

    const handleEscape = (event) => {
      if (event.key === 'Escape') {
        setShowCalendar(false)
      }
    }

    if (showCalendar) {
      document.addEventListener('mousedown', handleClickOutside)
      document.addEventListener('keydown', handleEscape)
      return () => {
        document.removeEventListener('mousedown', handleClickOutside)
        document.removeEventListener('keydown', handleEscape)
      }
    }
  }, [showCalendar])

  const handleDateRangeChange = (item) => {
    setDateRange([item.selection])
    setShowCalendar(false)
  }

  const formatDateRange = () => {
    const start = dateRange[0].startDate
    const end = dateRange[0].endDate
    return `${format(start, 'MMM dd, yyyy')} - ${format(end, 'MMM dd, yyyy')}`
  }

  const toggleCalendar = () => setShowCalendar(!showCalendar)

  return {
    dateRange,
    showCalendar,
    calendarRef,
    handleDateRangeChange,
    formatDateRange,
    toggleCalendar,
  }
} 