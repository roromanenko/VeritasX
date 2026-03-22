import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Bot } from 'lucide-react';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';

import { useApiProvider } from '../services/apiProvider';
import { BotCard } from '../components/BotCard';
import type { BotDto, BotStatus } from '../api';

export const BotMonitor = () => {
    const [bots, setBots] = useState<BotDto[]>([]);
    const [loading, setLoading] = useState(false);

    const navigate = useNavigate();
    const apiProvider = useApiProvider();

    const connectionRef = useRef<HubConnection | null>(null);
    const joinedGroupsRef = useRef<string[]>([]);

    useEffect(() => {
        fetchBots();
        connectHub();
        return () => { teardownHub(); };
    }, []);

    async function fetchBots() {
        setLoading(true);
        try {
            const response = await apiProvider.getBotsApi().apiBotsGet();
            const data = (response.data as unknown as { data: BotDto[] }).data ?? [];
            setBots(data);

            const ids = data.map(b => b.id).filter(Boolean) as string[];
            joinedGroupsRef.current = ids;

            const conn = connectionRef.current;
            if (conn?.state === HubConnectionState.Connected) {
                await Promise.all(ids.map(id => conn.invoke('JoinGroup', id)));
            }
        } finally {
            setLoading(false);
        }
    }

    async function connectHub() {
        const connection = apiProvider.getBotProgressHub();
        connectionRef.current = connection;

        connection.on('BotStatusChanged', (data: { botId?: string; status: string; error?: string }) => {
            if (!data.botId) return;
            setBots(prev => prev.map(b =>
                b.id === data.botId
                    ? { ...b, status: data.status as BotStatus, errorMessage: data.error }
                    : b
            ));
        });

        try {
            await connection.start();
            if (joinedGroupsRef.current.length > 0) {
                await Promise.all(joinedGroupsRef.current.map(id => connection.invoke('JoinGroup', id)));
            }
        } catch {
            // non-critical — cards still work without live updates
        }
    }

    async function teardownHub() {
        const connection = connectionRef.current;
        if (connection?.state === HubConnectionState.Connected) {
            try {
                await Promise.all(joinedGroupsRef.current.map(id => connection.invoke('LeaveGroup', id)));
                await connection.stop();
            } catch {
                // ignore on unmount
            }
        }
        connectionRef.current = null;
    }

    async function handleStart(id: string) {
        await apiProvider.getBotsApi().apiBotsIdStartPost(id);
    }

    async function handleStop(id: string) {
        await apiProvider.getBotsApi().apiBotsIdStopPost(id);
    }

    return (
        <main>
            <div className="page-header">
                <div className="page-header-text">
                    <h1>Trading Bots</h1>
                    <p>Monitor and manage your algorithmic trading bots</p>
                </div>
            </div>

            {bots.length > 0 ? (
                <div className="request-grid">
                    {bots.map(bot => (
                        <BotCard
                            key={bot.id}
                            bot={bot}
                            onClick={(id) => navigate(`/bots/${id}`)}
                            onStart={handleStart}
                            onStop={handleStop}
                        />
                    ))}
                </div>
            ) : (
                !loading && (
                    <div className="empty-state">
                        <Bot />
                        <p>No bots configured yet</p>
                    </div>
                )
            )}
        </main>
    );
};
