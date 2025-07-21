import { useEffect, useState } from "react"
import CandlestickChart from "../components/CandlestickChart"
import { CandleDto, DataCollectionJobDto } from "../api";
import { format } from "date-fns";
import { useParams } from "react-router-dom";
import { useApiProvider } from "../services/apiProvider";

export const Chart = () => {
    const { id } = useParams();
    if (!id) {
        return (
            <div>Id is required</div>
        )
    }
    const [loading, setLoading] = useState(false);
    const [candles, setCandels] = useState<CandleDto[]>([]);
    const [requestInfo, setRequestInfo] = useState<DataCollectionJobDto>({});

    const dataCollectionApi = useApiProvider().getDataCollectionApi();

    useEffect(() => {
        fetchRequestInfo()
    }, [])

    async function fetchRequestInfo() {
        setLoading(true)
        try {
            const infoResponse = await dataCollectionApi.apiDataCollectionJobsJobIdGet(id!);
            if (infoResponse.data) {
                setRequestInfo(infoResponse.data)
            }
            const dataResponse = await dataCollectionApi.apiDataCollectionDataJobIdGet(id!)
            if (dataResponse.data) {
                setCandels(dataResponse.data)
            }
        }
        finally {
            setLoading(false)
        }
    }

    return (
        <div>
            <div className="chart-container">
                {loading && <p>Loading chart...</p>}
                {!loading && candles.length > 0 ? (
                    <CandlestickChart candles={candles} symbol={requestInfo.symbol} />
                ) : !loading && (
                    <p>Data not found</p>
                )}
            </div>
            {candles.length > 0 && (
                <div className="stats">
                    <h3>Statistics</h3>
                    <p>Total candles: {candles.length}</p>
                    <p>Date range: {format(new Date(requestInfo.fromUtc!), 'MMM dd, yyyy')} - {format(new Date(requestInfo.toUtc!), 'MMM dd, yyyy')}</p>
                    <p>Interval: {requestInfo.interval}</p>
                </div>
            )}
        </div>
    )
}
