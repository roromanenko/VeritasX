import { DateRange } from "react-date-range"
import { useEffect, useRef, useState } from "react"
import { addDays, format, setDate } from "date-fns"

type DateRangeType = {
    startDate: Date,
    endDate: Date
}

type CustomDateRangeProps = {
    dateRange: DateRangeType,
    onDateRangeChange: (dateRange: DateRangeType) => void
}

export const CustomDateRange = ({ dateRange, onDateRangeChange }: CustomDateRangeProps) => {
    const calendarElementRef = useRef<HTMLDivElement>(null)
    const [showCalendar, setShowCalendar] = useState(false)

    const formatDateRange = () => {
        const start = dateRange.startDate
        const end = dateRange.endDate
        return `${format(start, 'MMM dd, yyyy')} - ${format(end, 'MMM dd, yyyy')}`
    }

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (calendarElementRef.current && !calendarElementRef.current.contains(event.target)) {
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

    return (
        <div className="date-input-wrapper" ref={calendarElementRef}>
            <input
                type="text"
                value={formatDateRange()}
                onClick={() => { setShowCalendar(!showCalendar) }}
                readOnly
                className="date-range-input"
                placeholder="Select date range..."
            />
            {showCalendar &&
                <div className="date-range-dropdown">
                    <DateRange
                        editableDateInputs={true}
                        onChange={(item) => { onDateRangeChange(item.range1) }}
                        moveRangeOnFirstSelection={true}
                        months={1}
                        direction="horizontal"
                        ranges={[dateRange]}
                        maxDate={new Date()}
                    />
                    <div>
                        <button>Apply</button>
                    </div>
                </div>}
        </div>
    )
}

export const useCustomDateRange = (defaultDateRange: DateRangeType | null = null): CustomDateRangeProps => {
    defaultDateRange = defaultDateRange ??
    {
        startDate: addDays(new Date(), -7),
        endDate: new Date(),
    };
    const [dateRange, setDateRange] = useState<DateRangeType>(defaultDateRange)
    return {
        dateRange,
        onDateRangeChange: (dateRage) => { setDateRange(dateRage) }
    }
}