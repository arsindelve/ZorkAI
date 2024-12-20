import {SessionHandler} from "./SessionHandler.ts";
import mixpanel from 'mixpanel-browser';

let sessionId = new SessionHandler();
mixpanel.init('62af8d500a6f295a71ef335b6ff56942');
mixpanel.identify(sessionId.getClientId());

let env_check = true;

let actions = {
    identify: (id) => {
        if (env_check) mixpanel.identify(id);
    },
    alias: (id) => {
        if (env_check) mixpanel.alias(id);
    },
    track: (name, props) => {
        if (env_check) mixpanel.track(name, props);
    },
    people: {
        set: (props) => {
            if (env_check) mixpanel.people.set(props);
        },
    },
};

export let Mixpanel = actions;