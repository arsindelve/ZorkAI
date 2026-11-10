import {WelcomeDialog} from '@zork-ai/shared-types';

export default function PlanetfallWelcomeDialog({
    open,
    handleClose,
}: {
    open: boolean;
    handleClose: () => void;
}) {
    return (
        <WelcomeDialog
            open={open}
            handleClose={handleClose}
            title="Welcome to Planetfall AI - A Modern Reimagining of the 1983 Classic!"
            aboutTitle="About Planetfall AI"
            paragraphs={[
                'This is a modern re-imagining of the beloved 1983 science fiction text adventure game Planetfall. You play as a lowly Ensign Seventh Class who crash-lands on the mysterious planet Resida after escaping the doomed spaceship Feinstein. While the story, puzzles, and humor remain faithful to the original, Planetfall AI introduces AI-powered parsing and dynamic responses for a richer, more immersive experience.',
                "The game understands natural language commands far beyond what the 1983 version could handle—including complex sentences, context-aware interactions, and even witty responses when you try something creative. And along the way, you'll meet Floyd—everyone's favorite robot companion.",
                'Planetfall AI is interactive fiction. You control the story by typing commands in plain English to explore locations, manipulate objects, solve puzzles, and interact with characters. To get started, type a command in the input box below and press Enter/Return. The game will process your input and continue the adventure based on your actions.',
            ]}
            commands={[
                'look',
                'take the kit',
                'go west',
                'ask floyd about the control panel',
                'blather, tell me about the feinstein',
            ]}
            commandsHeading="Need inspiration? Try:"
            attribution={
                <>
                    All credit for the original story, puzzles, and the Planetfall universe goes to{' '}
                    <a
                        href="https://en.wikipedia.org/wiki/Steve_Meretzky"
                        target="_blank"
                        style={{color: 'inherit', textDecoration: 'underline'}}
                    >
                        Steve Meretzky
                    </a>
                    , the brilliant mind behind this beloved 1983 Infocom classic.
                </>
            }
        />
    );
}
