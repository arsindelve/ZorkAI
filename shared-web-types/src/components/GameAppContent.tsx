import React, {useEffect, useState} from 'react';
import {QueryClient, QueryClientProvider} from '@tanstack/react-query';
import DialogType from '../DialogType';
import type {ISavedGame} from '../SavedGame';
import {Mixpanel} from '../utils/Mixpanel';
import PreferencesModal from '../modal/PreferencesModal';
import ReleaseNotesModal from '../modal/ReleaseNotesModal';
import RestartConfirmDialog from '../modal/RestartConfirmDialog';
import RestoreModal from '../modal/RestoreModal';
import SaveModal from '../modal/SaveModal';
import VideoDialog from '../modal/VideoModal';

export interface DialogServer {
    getSavedGames: (clientId: string) => Promise<ISavedGame[]>;
}

export interface GameAppContentProps {
    server: DialogServer;
    gameName: string;
    baseFontSizePx: number;
    hasLocationImages?: boolean;
    specimenCommand?: string;
    specimenRoom?: string;
    specimenBody?: string;
    clientId: string;
    dialogContext: {
        dialogToOpen: DialogType | undefined;
        setDialogToOpen: (dialog: DialogType | undefined) => void;
        setRestartGame: (restart: boolean) => void;
    };
    loadReleases: () => Promise<{date: string; name: string; notes: string}[]>;
    dialogs: {
        Restart: typeof RestartConfirmDialog;
        Restore: typeof RestoreModal;
        Save: typeof SaveModal;
        Video: typeof VideoDialog;
        ReleaseNotes: typeof ReleaseNotesModal;
        Preferences: typeof PreferencesModal;
    };
    renderGame: (latestVersion: string) => React.ReactNode;
    renderWelcome: (open: boolean, close: () => void, watchVideo: () => void) => React.ReactNode;
}

export default function GameAppContent({
    server,
    gameName,
    baseFontSizePx,
    hasLocationImages,
    specimenCommand,
    specimenRoom,
    specimenBody,
    clientId,
    dialogContext,
    loadReleases,
    dialogs,
    renderGame,
    renderWelcome,
}: GameAppContentProps) {
    const {
        Restart: RestartDialog,
        Restore: RestoreDialog,
        Save: SaveDialog,
        Video: VideoModal,
        ReleaseNotes: ReleaseNotesDialog,
        Preferences: PreferencesDialog,
    } = dialogs;
    const [queryClient] = useState(() => new QueryClient());
    const [savedGames, setSavedGames] = useState<ISavedGame[]>([]);
    const [openDialog, setOpenDialog] = useState<DialogType | undefined>();
    const [releases, setReleases] = useState<{date: string; name: string; notes: string}[]>([]);
    const [latestVersion, setLatestVersion] = useState('');
    const {dialogToOpen, setDialogToOpen, setRestartGame} = dialogContext;

    useEffect(() => {
        loadReleases()
            .then((data) => {
                setReleases(data);
                setLatestVersion(data[0]?.name ?? '');
            })
            .catch((error) => console.error('Error fetching releases:', error));
    }, [loadReleases]);

    useEffect(() => {
        if (!dialogToOpen) return;
        const open = async () => {
            if (dialogToOpen === DialogType.Save || dialogToOpen === DialogType.Restore) {
                setSavedGames(await server.getSavedGames(clientId));
            }
            if (dialogToOpen === DialogType.Video) Mixpanel.track('Open Video Dialog', {});
            if (dialogToOpen === DialogType.Welcome) Mixpanel.track('Open Welcome Dialog', {});
            if (dialogToOpen === DialogType.ReleaseNotes)
                Mixpanel.track('Open Release Notes Dialog', {});
            if (dialogToOpen === DialogType.Preferences)
                Mixpanel.track('Open Preferences Dialog', {});
            setOpenDialog(dialogToOpen);
            setDialogToOpen(undefined);
        };
        open().catch((error) => console.error('Error handling dialog:', error));
    }, [clientId, dialogToOpen, server, setDialogToOpen]);

    const close = () => setOpenDialog(undefined);
    const closeTracked = (name: string) => {
        close();
        Mixpanel.track(name, {});
    };

    return (
        <QueryClientProvider client={queryClient}>
            {renderGame(latestVersion)}
            <RestartDialog
                open={openDialog === DialogType.Restart}
                setOpen={(open) => !open && close()}
                onConfirm={() => setRestartGame(true)}
            />
            <RestoreDialog
                games={savedGames}
                open={openDialog === DialogType.Restore}
                setOpen={(open) => !open && close()}
            />
            <SaveDialog
                games={savedGames}
                open={openDialog === DialogType.Save}
                setOpen={(open) => !open && close()}
            />
            <VideoModal
                open={openDialog === DialogType.Video}
                handleClose={() => closeTracked('Close Video Dialog')}
            />
            <ReleaseNotesDialog
                open={openDialog === DialogType.ReleaseNotes}
                releases={releases}
                gameName={gameName}
                handleClose={() => closeTracked('Close Release Notes Dialog')}
            />
            <PreferencesDialog
                open={openDialog === DialogType.Preferences}
                baseFontSizePx={baseFontSizePx}
                hasLocationImages={hasLocationImages}
                specimenCommand={specimenCommand}
                specimenRoom={specimenRoom}
                specimenBody={specimenBody}
                handleClose={() => closeTracked('Close Preferences Dialog')}
            />
            {renderWelcome(
                openDialog === DialogType.Welcome,
                () => closeTracked('Close Welcome Dialog'),
                () => setOpenDialog(DialogType.Video),
            )}
        </QueryClientProvider>
    );
}
