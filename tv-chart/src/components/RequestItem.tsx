import { CollectionState, DataCollectionJobDto } from "../api";
import SpotlightCard from "../reactbits/Components/SpotlightCard/SpotlightCard";
import { useEffect, useRef, useState } from "react";
import { HubConnection, HubConnectionState } from '@microsoft/signalr';
import { useApiProvider } from "../services/apiProvider";

type RequestItemProps = {
    request: DataCollectionJobDto;
};


export const RequestItem = ({ request }: RequestItemProps) => {
    const [requestItem, setRequestItem] = useState<DataCollectionJobDto>(request);

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
                return 'inherit'
        }
    }
    const connection = useApiProvider().getJobProgressHub();
    useEffect(() => {
        const startConnection = async () => {
            try {
                await connection.start();
                console.log('SignalR Connected');

                // Join the job group
                await connection.invoke('JoinGroup', requestItem.id);

                // Listen for job updates
                connection.on('JobProgress', (update) => {
                    const newJobStatus = update as DataCollectionJobDto;
                    setRequestItem(newJobStatus);
                });

            } catch (err) {
                console.error('SignalR Connection Error: ', err);
            }
        };

        if (connection.state == HubConnectionState.Disconnected) {
            startConnection();
        }

        return () => {
            if (connection && connection.state == HubConnectionState.Connected) {
                connection.invoke('LeaveGroup', requestItem.id);
                connection.stop();
            }
        };
    }, [requestItem.id]);

    return (
        <>
            <SpotlightCard className="request-card" spotlightColor="rgba(0, 212, 170, 0.33)">
                <div className="request-row">
                    <div><strong>Symbol:</strong> {requestItem.symbol}</div>
                    <div><strong>Interval:</strong> {requestItem.interval}</div>
                    <div><strong>State:</strong> <span style={{ color: resolveStateColor(requestItem.state) }}>{requestItem.state}</span></div>
                </div>
                <div className="request-row">
                    <div><strong>Created:</strong> {formatDate(requestItem.createdAt)}</div>
                    <div><strong>Date Range:</strong> {formatDate(requestItem.fromUtc)} — {formatDate(requestItem.toUtc)}</div>
                    <div><strong>Chunks:</strong> {requestItem.completedChunks}/{requestItem.totalChunks}</div>
                </div>
                {requestItem.errorMessage && <div><div className="error request-error">Error: {requestItem.errorMessage}</div></div>}
            </SpotlightCard>
        </>
    )
}