import './App.css';
import {useState} from 'react';
import {
    GameAppContent,
    ReleaseNotesServer,
    ReleaseNotesModal,
    PreferencesModal,
    RestartConfirmDialog,
    RestoreModal,
    SaveModal,
    SessionHandler,
    useGameContext,
    VideoDialog,
} from '@zork-ai/shared-types';
import Game from './Game';
import GameMenu from './menu/GameMenu';
import WelcomeDialog from './modal/WelcomeModal';
import Server from './Server';
import {TRANSCRIPT_BASE_FONT_SIZE_PX} from './transcriptFontSize';

export default function App() {
    const [server] = useState(() => new Server());
    const [clientId] = useState(() => new SessionHandler().getClientId());
    const dialogContext = useGameContext();
    return (
        <div className="bg-[url('./back2.png')] bg-repeat bg-[size:500px_500px] h-screen overflow-hidden flex flex-col">
            <div className="flex-grow flex flex-col min-h-0 mt">
                <GameAppContent
                    server={server}
                    clientId={clientId}
                    dialogContext={dialogContext}
                    loadReleases={ReleaseNotesServer}
                    dialogs={{
                        Restart: RestartConfirmDialog,
                        Restore: RestoreModal,
                        Save: SaveModal,
                        Video: VideoDialog,
                        ReleaseNotes: ReleaseNotesModal,
                        Preferences: PreferencesModal,
                    }}
                    gameName="Zork AI"
                    baseFontSizePx={TRANSCRIPT_BASE_FONT_SIZE_PX}
                    hasLocationImages
                    renderGame={(latestVersion) => (
                        <>
                            <GameMenu latestVersion={latestVersion} />
                            <Game />
                        </>
                    )}
                    renderWelcome={(open, close, watchVideo) => (
                        <WelcomeDialog
                            open={open}
                            handleClose={close}
                            handleWatchVideo={watchVideo}
                        />
                    )}
                />
            </div>
        </div>
    );
}
