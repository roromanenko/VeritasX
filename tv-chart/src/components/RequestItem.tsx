import { ca } from "date-fns/locale";
import { CollectionState, DataCollectionJobDto } from "../api";
import { Color } from "ogl";
import SpotlightCard from "../reactbits/Components/SpotlightCard/SpotlightCard";



type RequestItemProps = {
    request: DataCollectionJobDto;
};


export const RequestItem = ({ request }: RequestItemProps) => {
    const {
        symbol,
        interval,
        fromUtc,
        toUtc,
        state,
        totalChunks,
        completedChunks,
        errorMessage
    } = request;

    const formatDate = (iso: string | undefined): string => {
        if (!iso)
            return '';

        return new Date(iso).toLocaleDateString(undefined, {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }

    const resolveStateColor = (state: CollectionState | undefined): string => {
        switch (state) {
            case CollectionState.Completed:
                return 'green'
            case CollectionState.Failed:
                return 'red'
            case CollectionState.InProgress:
                return 'yellow'
            default:
                return 'transparent'
        }
    }

    return (
        <>
            <SpotlightCard className="request-card" spotlightColor="rgba(0, 58, 47, 1)">
                <div className="request-row">
                    <div><strong>Symbol:</strong> {symbol}</div>
                    <div><strong>Interval:</strong> {interval}</div>
                    <div><strong>State:</strong> <span style={{ color: resolveStateColor(state) }}>{state}</span></div>
                </div>
                <div className="request-row">
                    <div><strong>Date Range:</strong> {formatDate(fromUtc)} — {formatDate(toUtc)}</div>
                    <div><strong>Chunks:</strong> {completedChunks}/{totalChunks}</div>
                    {errorMessage && <div className="error">Error: {errorMessage}</div>}
                </div>
            </SpotlightCard>
        </>
    )
}