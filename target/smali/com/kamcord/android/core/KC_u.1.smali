.class public final Lcom/kamcord/android/core/KC_u;
.super Lcom/kamcord/android/KC_ad;


# static fields
.field static b:Ljava/lang/Object;

.field public static c:Z

.field private static d:Z

.field private static n:Lcom/kamcord/android/FmodAudioSource;

.field private static o:Lcom/kamcord/android/KC_v;


# instance fields
.field private A:J

.field private B:J

.field private C:Z

.field private D:Lcom/kamcord/android/core/KC_s;

.field private E:Z

.field private F:Z

.field private e:Landroid/app/Activity;

.field private f:Lcom/kamcord/android/core/KC_D;

.field private g:Lcom/kamcord/android/core/KC_E;

.field private h:Lcom/kamcord/android/core/KC_A;

.field private i:Lcom/kamcord/android/core/KC_C;

.field private j:Lcom/kamcord/android/core/KC_B;

.field private k:Lcom/kamcord/android/core/KC_V;

.field private l:Lcom/kamcord/android/core/KC_H;

.field private m:Lcom/kamcord/android/AudioSource;

.field private p:Lcom/kamcord/android/core/KC_S;

.field private q:Lcom/kamcord/android/core/KC_T;

.field private r:Z

.field private s:Z

.field private t:D

.field private u:Z

.field private v:J

.field private w:I

.field private x:I

.field private y:J

.field private z:I


# direct methods
.method static constructor <clinit>()V
    .locals 1

    const/4 v0, 0x1

    sput-boolean v0, Lcom/kamcord/android/core/KC_u;->d:Z

    new-instance v0, Ljava/lang/Object;

    invoke-direct {v0}, Ljava/lang/Object;-><init>()V

    sput-object v0, Lcom/kamcord/android/core/KC_u;->b:Ljava/lang/Object;

    const/4 v0, 0x0

    sput-boolean v0, Lcom/kamcord/android/core/KC_u;->c:Z

    return-void
.end method

.method public constructor <init>()V
    .locals 6

    const-wide/16 v4, 0x0

    const/4 v2, 0x0

    const-string v0, "kamcord-worker"

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_ad;-><init>(Ljava/lang/String;)V

    iput-boolean v2, p0, Lcom/kamcord/android/core/KC_u;->r:Z

    iput-boolean v2, p0, Lcom/kamcord/android/core/KC_u;->s:Z

    const-wide/16 v0, 0x0

    iput-wide v0, p0, Lcom/kamcord/android/core/KC_u;->t:D

    iput-boolean v2, p0, Lcom/kamcord/android/core/KC_u;->u:Z

    iput-wide v4, p0, Lcom/kamcord/android/core/KC_u;->v:J

    iput v2, p0, Lcom/kamcord/android/core/KC_u;->w:I

    iput v2, p0, Lcom/kamcord/android/core/KC_u;->x:I

    iput-wide v4, p0, Lcom/kamcord/android/core/KC_u;->y:J

    iput v2, p0, Lcom/kamcord/android/core/KC_u;->z:I

    iput-wide v4, p0, Lcom/kamcord/android/core/KC_u;->A:J

    iput-wide v4, p0, Lcom/kamcord/android/core/KC_u;->B:J

    iput-boolean v2, p0, Lcom/kamcord/android/core/KC_u;->C:Z

    new-instance v0, Lcom/kamcord/android/core/KC_D;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_D;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_u;->f:Lcom/kamcord/android/core/KC_D;

    new-instance v0, Lcom/kamcord/android/core/KC_E;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_E;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_u;->g:Lcom/kamcord/android/core/KC_E;

    new-instance v0, Lcom/kamcord/android/core/KC_A;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_A;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_u;->h:Lcom/kamcord/android/core/KC_A;

    new-instance v0, Lcom/kamcord/android/core/KC_C;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_C;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_u;->i:Lcom/kamcord/android/core/KC_C;

    new-instance v0, Lcom/kamcord/android/core/KC_B;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_B;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_u;->j:Lcom/kamcord/android/core/KC_B;

    new-instance v0, Lcom/kamcord/android/core/KC_V;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_V;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_u;->k:Lcom/kamcord/android/core/KC_V;

    new-instance v0, Lcom/kamcord/android/KC_t;

    invoke-direct {v0}, Lcom/kamcord/android/KC_t;-><init>()V

    invoke-virtual {p0, v0}, Lcom/kamcord/android/core/KC_u;->setUncaughtExceptionHandler(Ljava/lang/Thread$UncaughtExceptionHandler;)V

    return-void
.end method

.method public static a([FI)V
    .locals 1

    :try_start_0
    sget-object v0, Lcom/kamcord/android/core/KC_u;->o:Lcom/kamcord/android/KC_v;

    if-nez v0, :cond_0

    const-string v0, "WriteAudioData called with no sample-rate and number-of-channels specified."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    :goto_0
    return-void

    :cond_0
    sget-object v0, Lcom/kamcord/android/core/KC_u;->o:Lcom/kamcord/android/KC_v;

    invoke-static {p0, p1}, Lcom/kamcord/android/KC_v;->a([FI)V
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    goto :goto_0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    const-string v0, "Unexpected error in worker thread. Source: 15"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V

    goto :goto_0
.end method

.method public static e()V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_a;->a()Z

    move-result v0

    if-eqz v0, :cond_1

    :try_start_0
    const-string v0, "com.unity3d.player.UnityPlayer"

    invoke-static {v0}, Ljava/lang/Class;->forName(Ljava/lang/String;)Ljava/lang/Class;

    move-result-object v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAutomaticAudioRecording()Z

    move-result v0

    if-eqz v0, :cond_0

    new-instance v0, Lcom/kamcord/android/FmodAudioSource;

    invoke-direct {v0}, Lcom/kamcord/android/FmodAudioSource;-><init>()V

    sput-object v0, Lcom/kamcord/android/core/KC_u;->n:Lcom/kamcord/android/FmodAudioSource;
    :try_end_0
    .catch Ljava/lang/ClassNotFoundException; {:try_start_0 .. :try_end_0} :catch_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_1

    :cond_0
    :goto_0
    :try_start_1
    sget-object v0, Lcom/kamcord/android/core/KC_u;->n:Lcom/kamcord/android/FmodAudioSource;
    :try_end_1
    .catch Ljava/lang/Throwable; {:try_start_1 .. :try_end_1} :catch_1

    if-eqz v0, :cond_1

    :try_start_2
    const-string v0, "Loading libkamcordaudio.so..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    const-string v0, "kamcordaudio"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    const-string v0, "...done loading libkamcordaudio.so."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_2
    .catch Ljava/lang/Exception; {:try_start_2 .. :try_end_2} :catch_2
    .catch Ljava/lang/UnsatisfiedLinkError; {:try_start_2 .. :try_end_2} :catch_3
    .catch Ljava/lang/Throwable; {:try_start_2 .. :try_end_2} :catch_1

    :cond_1
    :goto_1
    return-void

    :catch_0
    move-exception v0

    const/4 v0, 0x0

    :try_start_3
    sput-object v0, Lcom/kamcord/android/core/KC_u;->n:Lcom/kamcord/android/FmodAudioSource;
    :try_end_3
    .catch Ljava/lang/Throwable; {:try_start_3 .. :try_end_3} :catch_1

    goto :goto_0

    :catch_1
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    const-string v0, "Unexpected error in worker thread. Source: 1"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V

    goto :goto_1

    :catch_2
    move-exception v0

    :try_start_4
    const-string v1, "Loading libkamcordaudio.so failed."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    const/4 v0, 0x0

    sput-object v0, Lcom/kamcord/android/core/KC_u;->n:Lcom/kamcord/android/FmodAudioSource;

    goto :goto_1

    :catch_3
    move-exception v0

    const-string v1, "Linker error!"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/UnsatisfiedLinkError;->printStackTrace()V

    const/4 v0, 0x0

    sput-object v0, Lcom/kamcord/android/core/KC_u;->n:Lcom/kamcord/android/FmodAudioSource;
    :try_end_4
    .catch Ljava/lang/Throwable; {:try_start_4 .. :try_end_4} :catch_1

    goto :goto_1
.end method

.method private t()Lcom/kamcord/android/core/KC_f;
    .locals 4

    :try_start_0
    invoke-static {}, Lcom/kamcord/android/core/KC_s;->f()Lcom/kamcord/android/core/KC_f;

    move-result-object v0

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_u;->u()Lcom/kamcord/android/core/KC_s;

    move-result-object v1

    invoke-virtual {v1, v0}, Lcom/kamcord/android/core/KC_s;->a(Lcom/kamcord/android/core/KC_f;)I

    move-result v1

    if-eqz v1, :cond_0

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Initializing framebuffers with reason: "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_u;->u()Lcom/kamcord/android/core/KC_s;

    move-result-object v2

    invoke-virtual {v2, v0, v1}, Lcom/kamcord/android/core/KC_s;->a(Lcom/kamcord/android/core/KC_f;I)V

    const/4 v2, 0x1

    sput-boolean v2, Lcom/kamcord/android/core/KC_u;->c:Z

    and-int/lit8 v1, v1, 0x1

    if-nez v1, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->stopRecording()V
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :cond_0
    :goto_0
    return-object v0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    const-string v0, "Unexpected error in worker thread. Source: 11"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V

    const/4 v0, 0x0

    goto :goto_0
.end method

.method private u()Lcom/kamcord/android/core/KC_s;
    .locals 2

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->D:Lcom/kamcord/android/core/KC_s;

    if-nez v0, :cond_1

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_a;->c()Z

    move-result v0

    if-eqz v0, :cond_2

    const/4 v0, 0x1

    sget-boolean v1, Lcom/kamcord/android/core/KC_u;->d:Z

    if-eqz v1, :cond_0

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_a;->m()I

    move-result v0

    :cond_0
    new-instance v1, Lcom/kamcord/android/core/KC_r;

    invoke-direct {v1, v0}, Lcom/kamcord/android/core/KC_r;-><init>(I)V

    iput-object v1, p0, Lcom/kamcord/android/core/KC_u;->D:Lcom/kamcord/android/core/KC_s;

    :cond_1
    :goto_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->D:Lcom/kamcord/android/core/KC_s;

    :goto_1
    return-object v0

    :cond_2
    new-instance v0, Lcom/kamcord/android/core/KC_q;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_q;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_u;->D:Lcom/kamcord/android/core/KC_s;
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    goto :goto_0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    const-string v0, "Unexpected error in worker thread. Source: 13"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V

    const/4 v0, 0x0

    goto :goto_1
.end method


# virtual methods
.method protected final a()Lcom/kamcord/android/KC_ac;
    .locals 2

    new-instance v0, Lcom/kamcord/android/core/KC_t;

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_u;->getLooper()Landroid/os/Looper;

    move-result-object v1

    invoke-direct {v0, v1, p0}, Lcom/kamcord/android/core/KC_t;-><init>(Landroid/os/Looper;Lcom/kamcord/android/core/KC_u;)V

    return-object v0
.end method

.method final a(I)V
    .locals 5

    const/4 v4, 0x1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->fatalError()Z

    move-result v0

    if-eqz v0, :cond_1

    if-eq p1, v4, :cond_1

    const-string v0, "Not showing UI because of fatal error."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {v4}, Lcom/kamcord/android/Kamcord;->notifyViewDidNotAppear(Z)V

    :cond_0
    :goto_0
    return-void

    :cond_1
    if-nez p1, :cond_3

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->l:Lcom/kamcord/android/core/KC_H;

    if-eqz v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->l:Lcom/kamcord/android/core/KC_H;

    iget-boolean v0, v0, Lcom/kamcord/android/core/KC_H;->d:Z

    if-eqz v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_T;->h()Z

    move-result v0

    if-eqz v0, :cond_3

    :cond_2
    const-string v0, "Not showing share UI because the video isn\'t complete!"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {v4}, Lcom/kamcord/android/Kamcord;->notifyViewDidNotAppear(Z)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->videoIncompleteWarningEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    new-instance v0, Lcom/kamcord/android/KC_l;

    invoke-direct {v0}, Lcom/kamcord/android/KC_l;-><init>()V

    :try_start_0
    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->e:Landroid/app/Activity;

    invoke-virtual {v1}, Landroid/app/Activity;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v1

    const-string v2, "InvalidVideoDialogFragment"

    invoke-virtual {v0, v1, v2}, Landroid/app/DialogFragment;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V
    :try_end_0
    .catch Ljava/lang/IllegalStateException; {:try_start_0 .. :try_end_0} :catch_0

    goto :goto_0

    :catch_0
    move-exception v0

    const-string v1, "Can\'t show invalid video dialog fragment, is onSavedInstanceState being handled correctly?"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/IllegalStateException;->printStackTrace()V

    goto :goto_0

    :cond_3
    if-nez p1, :cond_4

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-nez v0, :cond_5

    :cond_4
    if-ne p1, v4, :cond_6

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_a;->b()Z

    move-result v0

    if-eqz v0, :cond_6

    :cond_5
    invoke-static {}, Lcom/kamcord/android/Kamcord;->notifyViewDidAppear()V

    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->i:Lcom/kamcord/android/core/KC_C;

    monitor-enter v1

    :try_start_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->i:Lcom/kamcord/android/core/KC_C;

    const/4 v2, 0x1

    iput v2, v0, Lcom/kamcord/android/core/KC_C;->a:I
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    :try_start_2
    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-nez v0, :cond_7

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->i:Lcom/kamcord/android/core/KC_C;

    const-wide/16 v2, 0x64

    invoke-virtual {v0, v2, v3}, Ljava/lang/Object;->wait(J)V
    :try_end_2
    .catch Ljava/lang/InterruptedException; {:try_start_2 .. :try_end_2} :catch_1
    .catchall {:try_start_2 .. :try_end_2} :catchall_0

    :goto_1
    :try_start_3
    monitor-exit v1
    :try_end_3
    .catchall {:try_start_3 .. :try_end_3} :catchall_0

    packed-switch p1, :pswitch_data_0

    goto :goto_0

    :pswitch_0
    new-instance v0, Landroid/content/Intent;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->e:Landroid/app/Activity;

    const-class v2, Lcom/kamcord/android/KamcordActivity;

    invoke-direct {v0, v1, v2}, Landroid/content/Intent;-><init>(Landroid/content/Context;Ljava/lang/Class;)V

    const-string v1, "mode"

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;I)Landroid/content/Intent;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->e:Landroid/app/Activity;

    invoke-virtual {v1, v0}, Landroid/app/Activity;->startActivity(Landroid/content/Intent;)V

    goto/16 :goto_0

    :cond_6
    invoke-static {v4}, Lcom/kamcord/android/Kamcord;->notifyViewDidNotAppear(Z)V

    goto/16 :goto_0

    :cond_7
    :try_start_4
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->i:Lcom/kamcord/android/core/KC_C;

    invoke-virtual {v0}, Ljava/lang/Object;->wait()V
    :try_end_4
    .catch Ljava/lang/InterruptedException; {:try_start_4 .. :try_end_4} :catch_1
    .catchall {:try_start_4 .. :try_end_4} :catchall_0

    goto :goto_1

    :catch_1
    move-exception v0

    :try_start_5
    const-string v2, "Kamcord-worker-thread communication with GPU thread interrupted during showView."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/InterruptedException;->printStackTrace()V
    :try_end_5
    .catchall {:try_start_5 .. :try_end_5} :catchall_0

    goto :goto_1

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0

    :pswitch_1
    new-instance v0, Landroid/content/Intent;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->e:Landroid/app/Activity;

    const-class v2, Lcom/kamcord/android/KamcordActivity;

    invoke-direct {v0, v1, v2}, Landroid/content/Intent;-><init>(Landroid/content/Context;Ljava/lang/Class;)V

    const-string v1, "mode"

    invoke-virtual {v0, v1, v4}, Landroid/content/Intent;->putExtra(Ljava/lang/String;I)Landroid/content/Intent;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->e:Landroid/app/Activity;

    invoke-virtual {v1, v0}, Landroid/app/Activity;->startActivity(Landroid/content/Intent;)V

    goto/16 :goto_0

    nop

    :pswitch_data_0
    .packed-switch 0x0
        :pswitch_0
        :pswitch_1
    .end packed-switch
.end method

.method public final a(II)V
    .locals 1

    :try_start_0
    sget-object v0, Lcom/kamcord/android/core/KC_u;->o:Lcom/kamcord/android/KC_v;

    if-nez v0, :cond_0

    new-instance v0, Lcom/kamcord/android/KC_v;

    invoke-direct {v0, p1, p2}, Lcom/kamcord/android/KC_v;-><init>(II)V

    sput-object v0, Lcom/kamcord/android/core/KC_u;->o:Lcom/kamcord/android/KC_v;

    :cond_0
    sget-object v0, Lcom/kamcord/android/core/KC_u;->o:Lcom/kamcord/android/KC_v;

    invoke-virtual {p0, v0}, Lcom/kamcord/android/core/KC_u;->a(Lcom/kamcord/android/AudioSource;)V
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    return-void

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    const-string v0, "Unexpected error in worker thread. Source: 14"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V

    goto :goto_0
.end method

.method public final a(Landroid/app/Activity;)V
    .locals 1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->e:Landroid/app/Activity;

    if-eqz v0, :cond_0

    const-string v0, "Activity set with activity already non-null."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    :cond_0
    iput-object p1, p0, Lcom/kamcord/android/core/KC_u;->e:Landroid/app/Activity;
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    return-void

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    const-string v0, "Unexpected error in worker thread. Source: 2"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V

    goto :goto_0
.end method

.method public final a(Lcom/kamcord/android/AudioSource;)V
    .locals 1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->m:Lcom/kamcord/android/AudioSource;

    if-eqz v0, :cond_0

    const-string v0, "Attempt to add more than one audio source.  Replacing old one."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    :cond_0
    iput-object p1, p0, Lcom/kamcord/android/core/KC_u;->m:Lcom/kamcord/android/AudioSource;
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    return-void

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    const-string v0, "Unexpected error in worker thread. Source: 5"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V

    goto :goto_0
.end method

.method final a(Z)V
    .locals 3

    const/4 v1, 0x1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->fatalError()Z

    move-result v0

    if-eqz v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    const-string v0, "stopRecording called."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    if-eqz v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/core/KC_T;->a(Z)V

    :cond_2
    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->g:Lcom/kamcord/android/core/KC_E;

    monitor-enter v1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->g:Lcom/kamcord/android/core/KC_E;

    const/4 v2, 0x1

    iput-boolean v2, v0, Lcom/kamcord/android/core/KC_E;->a:Z
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :try_start_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->g:Lcom/kamcord/android/core/KC_E;

    invoke-virtual {v0}, Ljava/lang/Object;->wait()V
    :try_end_1
    .catch Ljava/lang/InterruptedException; {:try_start_1 .. :try_end_1} :catch_0
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    :goto_1
    :try_start_2
    monitor-exit v1
    :try_end_2
    .catchall {:try_start_2 .. :try_end_2} :catchall_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->p:Lcom/kamcord/android/core/KC_S;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->p:Lcom/kamcord/android/core/KC_S;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_S;->a:Lcom/kamcord/android/core/KC_H;

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->setLastVideo(Lcom/kamcord/android/core/KC_H;)V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->p:Lcom/kamcord/android/core/KC_S;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_S;->a:Lcom/kamcord/android/core/KC_H;

    iput-object v0, p0, Lcom/kamcord/android/core/KC_u;->l:Lcom/kamcord/android/core/KC_H;

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/core/KC_u;->p:Lcom/kamcord/android/core/KC_S;

    goto :goto_0

    :catch_0
    move-exception v0

    :try_start_3
    const-string v2, "Kamcord-worker-thread communication with GPU thread interrupted."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/InterruptedException;->printStackTrace()V
    :try_end_3
    .catchall {:try_start_3 .. :try_end_3} :catchall_0

    goto :goto_1

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method public final b(Z)V
    .locals 0

    iput-boolean p1, p0, Lcom/kamcord/android/core/KC_u;->u:Z

    return-void
.end method

.method public final f()Landroid/graphics/Bitmap;
    .locals 4

    const/4 v1, 0x0

    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->j:Lcom/kamcord/android/core/KC_B;

    monitor-enter v2

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->j:Lcom/kamcord/android/core/KC_B;

    const/4 v3, 0x1

    iput v3, v0, Lcom/kamcord/android/core/KC_B;->a:I
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :try_start_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->j:Lcom/kamcord/android/core/KC_B;

    invoke-virtual {v0}, Ljava/lang/Object;->wait()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->j:Lcom/kamcord/android/core/KC_B;

    iget-object v1, v0, Lcom/kamcord/android/core/KC_B;->b:Landroid/graphics/Bitmap;

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->j:Lcom/kamcord/android/core/KC_B;

    const/4 v3, 0x0

    iput-object v3, v0, Lcom/kamcord/android/core/KC_B;->b:Landroid/graphics/Bitmap;
    :try_end_1
    .catch Ljava/lang/InterruptedException; {:try_start_1 .. :try_end_1} :catch_0
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    :goto_0
    :try_start_2
    monitor-exit v2

    return-object v1

    :catch_0
    move-exception v0

    const-string v3, "Communication with GPU thread interrupted during screenshot."

    invoke-static {v3}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/InterruptedException;->printStackTrace()V
    :try_end_2
    .catchall {:try_start_2 .. :try_end_2} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v2

    throw v0
.end method

.method public final g()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->a:Lcom/kamcord/android/KC_ac;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->a:Lcom/kamcord/android/KC_ac;

    const/4 v2, 0x1

    invoke-static {v1, v2}, Landroid/os/Message;->obtain(Landroid/os/Handler;I)Landroid/os/Message;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    return-void
.end method

.method public final h()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->a:Lcom/kamcord/android/KC_ac;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->a:Lcom/kamcord/android/KC_ac;

    const/4 v2, 0x4

    invoke-static {v1, v2}, Landroid/os/Message;->obtain(Landroid/os/Handler;I)Landroid/os/Message;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    return-void
.end method

.method public final i()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->a:Lcom/kamcord/android/KC_ac;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->a:Lcom/kamcord/android/KC_ac;

    const/4 v2, 0x2

    invoke-static {v1, v2}, Landroid/os/Message;->obtain(Landroid/os/Handler;I)Landroid/os/Message;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    return-void
.end method

.method public final j()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->a:Lcom/kamcord/android/KC_ac;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->a:Lcom/kamcord/android/KC_ac;

    const/4 v2, 0x3

    invoke-static {v1, v2}, Landroid/os/Message;->obtain(Landroid/os/Handler;I)Landroid/os/Message;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    return-void
.end method

.method public final k()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->a:Lcom/kamcord/android/KC_ac;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->a:Lcom/kamcord/android/KC_ac;

    const/4 v2, 0x5

    invoke-static {v1, v2}, Landroid/os/Message;->obtain(Landroid/os/Handler;I)Landroid/os/Message;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    return-void
.end method

.method public final l()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->a:Lcom/kamcord/android/KC_ac;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->a:Lcom/kamcord/android/KC_ac;

    const/4 v2, 0x6

    invoke-static {v1, v2}, Landroid/os/Message;->obtain(Landroid/os/Handler;I)Landroid/os/Message;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    return-void
.end method

.method public final m()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->a:Lcom/kamcord/android/KC_ac;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->a:Lcom/kamcord/android/KC_ac;

    const/4 v2, 0x7

    invoke-static {v1, v2}, Landroid/os/Message;->obtain(Landroid/os/Handler;I)Landroid/os/Message;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    return-void
.end method

.method final n()V
    .locals 5

    const/4 v0, 0x1

    const/4 v1, 0x0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->fatalError()Z

    move-result v2

    if-eqz v2, :cond_0

    :goto_0
    return-void

    :cond_0
    const-string v2, "startRecording called."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-boolean v2, p0, Lcom/kamcord/android/core/KC_u;->r:Z

    if-nez v2, :cond_1

    invoke-static {}, Lcom/kamcord/android/core/KamcordNative;->initDefaultFramebuffer()V

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_u;->r:Z

    :cond_1
    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->p:Lcom/kamcord/android/core/KC_S;

    if-nez v2, :cond_3

    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->f:Lcom/kamcord/android/core/KC_D;

    monitor-enter v2

    :try_start_0
    iget-object v3, p0, Lcom/kamcord/android/core/KC_u;->f:Lcom/kamcord/android/core/KC_D;

    const/4 v4, 0x1

    iput-boolean v4, v3, Lcom/kamcord/android/core/KC_D;->a:Z
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :try_start_1
    iget-object v3, p0, Lcom/kamcord/android/core/KC_u;->f:Lcom/kamcord/android/core/KC_D;

    invoke-virtual {v3}, Ljava/lang/Object;->wait()V

    const-string v3, "Creating new Video object."

    invoke-static {v3}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v3, p0, Lcom/kamcord/android/core/KC_u;->f:Lcom/kamcord/android/core/KC_D;

    iget-object v3, v3, Lcom/kamcord/android/core/KC_D;->b:Lcom/kamcord/android/core/KC_T;

    iput-object v3, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    iget-object v3, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v3}, Lcom/kamcord/android/core/KC_T;->i()Z

    move-result v3

    if-nez v3, :cond_2

    const-string v0, "Video codec failed to init"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V

    const-string v0, "No video codec, worker shutting down before start recording."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_1
    .catch Ljava/lang/InterruptedException; {:try_start_1 .. :try_end_1} :catch_0
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    :try_start_2
    monitor-exit v2
    :try_end_2
    .catchall {:try_start_2 .. :try_end_2} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v2

    throw v0

    :cond_2
    :try_start_3
    iget-object v3, p0, Lcom/kamcord/android/core/KC_u;->f:Lcom/kamcord/android/core/KC_D;

    const/4 v4, 0x0

    iput-object v4, v3, Lcom/kamcord/android/core/KC_D;->b:Lcom/kamcord/android/core/KC_T;

    sget v3, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v4, 0x12

    if-lt v3, v4, :cond_4

    :goto_1
    if-eqz v0, :cond_5

    new-instance v0, Lcom/kamcord/android/core/KC_Q;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    invoke-direct {v0, v1}, Lcom/kamcord/android/core/KC_Q;-><init>(Lcom/kamcord/android/core/KC_T;)V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_u;->p:Lcom/kamcord/android/core/KC_S;

    :goto_2
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->p:Lcom/kamcord/android/core/KC_S;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_S;->a:Lcom/kamcord/android/core/KC_H;

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->setCurrentVideo(Lcom/kamcord/android/core/KC_H;)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->voiceOverlayEnabled()Z

    move-result v0

    if-eqz v0, :cond_6

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    const-string v1, "voice_overlay_enabled"

    const/4 v3, 0x0

    invoke-interface {v0, v1, v3}, Landroid/content/SharedPreferences;->getBoolean(Ljava/lang/String;Z)Z

    move-result v0

    if-eqz v0, :cond_6

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_T;->a()V

    :goto_3
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_T;->b()V

    const/4 v0, 0x0

    invoke-static {v0}, Lcom/kamcord/android/KC_m;->a(I)V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->p:Lcom/kamcord/android/core/KC_S;

    if-nez v0, :cond_7

    const-string v0, "Video object null."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_3
    .catch Ljava/lang/InterruptedException; {:try_start_3 .. :try_end_3} :catch_0
    .catchall {:try_start_3 .. :try_end_3} :catchall_0

    :goto_4
    :try_start_4
    monitor-exit v2
    :try_end_4
    .catchall {:try_start_4 .. :try_end_4} :catchall_0

    :cond_3
    const-string v0, "...done starting recording."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    goto/16 :goto_0

    :cond_4
    move v0, v1

    goto :goto_1

    :cond_5
    :try_start_5
    new-instance v0, Lcom/kamcord/android/core/KC_P;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    invoke-direct {v0, v1}, Lcom/kamcord/android/core/KC_P;-><init>(Lcom/kamcord/android/core/KC_T;)V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_u;->p:Lcom/kamcord/android/core/KC_S;
    :try_end_5
    .catch Ljava/lang/InterruptedException; {:try_start_5 .. :try_end_5} :catch_0
    .catchall {:try_start_5 .. :try_end_5} :catchall_0

    goto :goto_2

    :catch_0
    move-exception v0

    :try_start_6
    const-string v1, "Kamcord-worker-thread communication with GPU thread interrupted."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/InterruptedException;->printStackTrace()V
    :try_end_6
    .catchall {:try_start_6 .. :try_end_6} :catchall_0

    goto :goto_4

    :cond_6
    :try_start_7
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    const/4 v1, 0x0

    iput-object v1, v0, Lcom/kamcord/android/core/KC_T;->c:Lcom/kamcord/android/core/KC_U;

    goto :goto_3

    :cond_7
    const-string v0, "Video object created."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_7
    .catch Ljava/lang/InterruptedException; {:try_start_7 .. :try_end_7} :catch_0
    .catchall {:try_start_7 .. :try_end_7} :catchall_0

    goto :goto_4
.end method

.method final o()V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->fatalError()Z

    move-result v0

    if-eqz v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->p:Lcom/kamcord/android/core/KC_S;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_T;->c()V

    goto :goto_0
.end method

.method final p()V
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->fatalError()Z

    move-result v0

    if-eqz v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->p:Lcom/kamcord/android/core/KC_S;

    if-eqz v0, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->h:Lcom/kamcord/android/core/KC_A;

    monitor-enter v1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->h:Lcom/kamcord/android/core/KC_A;

    const/4 v2, 0x1

    iput-boolean v2, v0, Lcom/kamcord/android/core/KC_A;->a:Z
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :try_start_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->h:Lcom/kamcord/android/core/KC_A;

    invoke-virtual {v0}, Ljava/lang/Object;->wait()V
    :try_end_1
    .catch Ljava/lang/InterruptedException; {:try_start_1 .. :try_end_1} :catch_0
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    :goto_1
    :try_start_2
    monitor-exit v1
    :try_end_2
    .catchall {:try_start_2 .. :try_end_2} :catchall_0

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_u;->u()Lcom/kamcord/android/core/KC_s;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_s;->b()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_T;->d()V

    goto :goto_0

    :catch_0
    move-exception v0

    :try_start_3
    const-string v2, "Kamcord-worker-thread communication with GPU thread interrupted."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/InterruptedException;->printStackTrace()V
    :try_end_3
    .catchall {:try_start_3 .. :try_end_3} :catchall_0

    goto :goto_1

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method public final q()Z
    .locals 14

    const-wide v12, 0x412e848000000000L    # 1000000.0

    const/4 v1, 0x1

    const/4 v0, 0x0

    :try_start_0
    iget-boolean v2, p0, Lcom/kamcord/android/core/KC_u;->u:Z

    if-eqz v2, :cond_1

    invoke-static {}, Ljava/lang/System;->nanoTime()J

    move-result-wide v2

    iget-wide v4, p0, Lcom/kamcord/android/core/KC_u;->B:J

    const-wide/16 v6, 0x0

    cmp-long v4, v4, v6

    if-eqz v4, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isRecording()Z

    move-result v4

    if-eqz v4, :cond_b

    iget-wide v4, p0, Lcom/kamcord/android/core/KC_u;->y:J

    iget-wide v6, p0, Lcom/kamcord/android/core/KC_u;->B:J

    sub-long v6, v2, v6

    add-long/2addr v4, v6

    iput-wide v4, p0, Lcom/kamcord/android/core/KC_u;->y:J

    iget v4, p0, Lcom/kamcord/android/core/KC_u;->x:I

    add-int/lit8 v4, v4, 0x1

    iput v4, p0, Lcom/kamcord/android/core/KC_u;->x:I

    :cond_0
    :goto_0
    iput-wide v2, p0, Lcom/kamcord/android/core/KC_u;->B:J

    iget v4, p0, Lcom/kamcord/android/core/KC_u;->w:I

    add-int/lit8 v4, v4, 0x1

    iput v4, p0, Lcom/kamcord/android/core/KC_u;->w:I

    iget v4, p0, Lcom/kamcord/android/core/KC_u;->w:I

    rem-int/lit16 v4, v4, 0xc8

    iput v4, p0, Lcom/kamcord/android/core/KC_u;->w:I

    iget v4, p0, Lcom/kamcord/android/core/KC_u;->w:I

    const/16 v5, 0xc7

    if-ne v4, v5, :cond_1

    iget-wide v4, p0, Lcom/kamcord/android/core/KC_u;->v:J

    sub-long v4, v2, v4

    long-to-double v4, v4

    const-wide v6, 0x41a7d78400000000L    # 2.0E8

    div-double/2addr v4, v6

    iget-wide v6, p0, Lcom/kamcord/android/core/KC_u;->A:J

    long-to-double v6, v6

    iget v8, p0, Lcom/kamcord/android/core/KC_u;->z:I

    int-to-double v8, v8

    mul-double/2addr v8, v12

    div-double/2addr v6, v8

    iget-wide v8, p0, Lcom/kamcord/android/core/KC_u;->y:J

    long-to-double v8, v8

    iget v10, p0, Lcom/kamcord/android/core/KC_u;->x:I

    int-to-double v10, v10

    mul-double/2addr v10, v12

    div-double/2addr v8, v10

    sub-double v10, v8, v6

    new-instance v12, Ljava/lang/StringBuilder;

    const-string v13, "interval: "

    invoke-direct {v12, v13}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v12, v4, v5}, Ljava/lang/StringBuilder;->append(D)Ljava/lang/StringBuilder;

    move-result-object v4

    const-string v5, " sans-rec: "

    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4, v6, v7}, Ljava/lang/StringBuilder;->append(D)Ljava/lang/StringBuilder;

    move-result-object v4

    const-string v5, " with-rec: "

    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4, v8, v9}, Ljava/lang/StringBuilder;->append(D)Ljava/lang/StringBuilder;

    move-result-object v4

    const-string v5, " penalty: "

    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4, v10, v11}, Ljava/lang/StringBuilder;->append(D)Ljava/lang/StringBuilder;

    move-result-object v4

    const-string v5, "\n"

    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-static {v4}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iput-wide v2, p0, Lcom/kamcord/android/core/KC_u;->v:J
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :cond_1
    :goto_1
    :try_start_1
    invoke-static {}, Lcom/kamcord/android/Kamcord;->getDelayAllocation()Z

    move-result v2

    if-eqz v2, :cond_2

    iget-boolean v2, p0, Lcom/kamcord/android/core/KC_u;->s:Z

    if-eqz v2, :cond_3

    :cond_2
    invoke-direct {p0}, Lcom/kamcord/android/core/KC_u;->t()Lcom/kamcord/android/core/KC_f;

    :cond_3
    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->i:Lcom/kamcord/android/core/KC_C;

    monitor-enter v2
    :try_end_1
    .catch Ljava/lang/Throwable; {:try_start_1 .. :try_end_1} :catch_1

    :try_start_2
    iget-object v3, p0, Lcom/kamcord/android/core/KC_u;->i:Lcom/kamcord/android/core/KC_C;

    iget v3, v3, Lcom/kamcord/android/core/KC_C;->a:I

    const/4 v4, 0x2

    if-ne v3, v4, :cond_4

    iget-object v3, p0, Lcom/kamcord/android/core/KC_u;->i:Lcom/kamcord/android/core/KC_C;

    const/4 v4, 0x0

    iput v4, v3, Lcom/kamcord/android/core/KC_C;->a:I

    iget-object v3, p0, Lcom/kamcord/android/core/KC_u;->i:Lcom/kamcord/android/core/KC_C;

    invoke-virtual {v3}, Ljava/lang/Object;->notify()V

    :cond_4
    monitor-exit v2
    :try_end_2
    .catchall {:try_start_2 .. :try_end_2} :catchall_0

    :try_start_3
    iget-object v3, p0, Lcom/kamcord/android/core/KC_u;->f:Lcom/kamcord/android/core/KC_D;

    monitor-enter v3
    :try_end_3
    .catch Ljava/lang/Throwable; {:try_start_3 .. :try_end_3} :catch_1

    :try_start_4
    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->f:Lcom/kamcord/android/core/KC_D;

    iget-boolean v2, v2, Lcom/kamcord/android/core/KC_D;->a:Z

    if-eqz v2, :cond_8

    const/4 v2, 0x1

    iput-boolean v2, p0, Lcom/kamcord/android/core/KC_u;->s:Z

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_u;->t()Lcom/kamcord/android/core/KC_f;

    move-result-object v4

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_u;->u()Lcom/kamcord/android/core/KC_s;

    move-result-object v2

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_s;->a()V

    const-string v2, "Restarting writer..."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_4
    .catchall {:try_start_4 .. :try_end_4} :catchall_1

    :try_start_5
    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    if-nez v2, :cond_5

    new-instance v2, Lcom/kamcord/android/core/KC_T;

    invoke-direct {v2}, Lcom/kamcord/android/core/KC_T;-><init>()V

    iput-object v2, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    :cond_5
    invoke-direct {p0}, Lcom/kamcord/android/core/KC_u;->u()Lcom/kamcord/android/core/KC_s;

    move-result-object v2

    iget-object v5, v2, Lcom/kamcord/android/core/KC_s;->a:Lcom/kamcord/android/core/KC_x;

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v2

    invoke-virtual {v2}, Lcom/kamcord/android/a/KC_a;->c()Z

    move-result v2

    if-eqz v2, :cond_d

    sget-boolean v2, Lcom/kamcord/android/core/KC_u;->d:Z

    if-eqz v2, :cond_c

    new-instance v2, Lcom/kamcord/android/core/KC_N;

    iget-object v6, p0, Lcom/kamcord/android/core/KC_u;->k:Lcom/kamcord/android/core/KC_V;

    invoke-direct {v2, v5, v6}, Lcom/kamcord/android/core/KC_N;-><init>(Lcom/kamcord/android/core/KC_x;Lcom/kamcord/android/core/KC_V;)V

    :goto_2
    new-instance v5, Lcom/kamcord/android/core/KC_a;

    invoke-direct {v5}, Lcom/kamcord/android/core/KC_a;-><init>()V

    iget-object v6, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    iget-object v7, p0, Lcom/kamcord/android/core/KC_u;->m:Lcom/kamcord/android/AudioSource;

    invoke-virtual {v6, v2, v4, v5, v7}, Lcom/kamcord/android/core/KC_T;->a(Lcom/kamcord/android/core/KC_I;Lcom/kamcord/android/core/KC_f;Lcom/kamcord/android/core/KC_a;Lcom/kamcord/android/AudioSource;)V

    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_T;->i()Z

    move-result v2

    if-nez v2, :cond_6

    const-string v2, "No video codec available."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V

    :cond_6
    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->m:Lcom/kamcord/android/AudioSource;

    if-eqz v2, :cond_7

    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_T;->i()Z

    move-result v2

    if-eqz v2, :cond_7

    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_T;->j()Z

    move-result v2

    if-nez v2, :cond_7

    const-string v2, "Kamcord: No audio codec available.  Attempting to proceed with video only."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    :cond_7
    const-string v2, "...done restarting writer."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_5
    .catch Ljava/lang/Throwable; {:try_start_5 .. :try_end_5} :catch_2
    .catchall {:try_start_5 .. :try_end_5} :catchall_1

    :goto_3
    :try_start_6
    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->f:Lcom/kamcord/android/core/KC_D;

    const/4 v4, 0x0

    iput-boolean v4, v2, Lcom/kamcord/android/core/KC_D;->a:Z

    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->f:Lcom/kamcord/android/core/KC_D;

    iget-object v4, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    iput-object v4, v2, Lcom/kamcord/android/core/KC_D;->b:Lcom/kamcord/android/core/KC_T;

    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->f:Lcom/kamcord/android/core/KC_D;

    invoke-virtual {v2}, Ljava/lang/Object;->notify()V

    :cond_8
    monitor-exit v3
    :try_end_6
    .catchall {:try_start_6 .. :try_end_6} :catchall_1

    :try_start_7
    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    if-eqz v2, :cond_9

    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_T;->h()Z

    move-result v2

    iput-boolean v2, p0, Lcom/kamcord/android/core/KC_u;->E:Z

    :cond_9
    iget-boolean v2, p0, Lcom/kamcord/android/core/KC_u;->E:Z

    if-eqz v2, :cond_a

    invoke-static {}, Ljava/lang/System;->nanoTime()J

    move-result-wide v2

    long-to-double v2, v2

    const-wide v4, 0x3e112e0be826d695L    # 1.0E-9

    mul-double/2addr v4, v2

    iget-wide v2, p0, Lcom/kamcord/android/core/KC_u;->t:D

    sub-double v2, v4, v2

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v6

    invoke-virtual {v6}, Lcom/kamcord/android/a/KC_a;->n()D

    move-result-wide v6

    cmpl-double v2, v2, v6

    if-lez v2, :cond_f

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_u;->u()Lcom/kamcord/android/core/KC_s;

    move-result-object v2

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_s;->h()Z

    move-result v2

    if-eqz v2, :cond_f

    move v2, v1

    :goto_4
    iput-boolean v2, p0, Lcom/kamcord/android/core/KC_u;->F:Z

    iget-boolean v2, p0, Lcom/kamcord/android/core/KC_u;->F:Z

    if-eqz v2, :cond_a

    iput-wide v4, p0, Lcom/kamcord/android/core/KC_u;->t:D
    :try_end_7
    .catch Ljava/lang/Throwable; {:try_start_7 .. :try_end_7} :catch_1

    :cond_a
    move v0, v1

    :goto_5
    return v0

    :cond_b
    :try_start_8
    iget-wide v4, p0, Lcom/kamcord/android/core/KC_u;->A:J

    iget-wide v6, p0, Lcom/kamcord/android/core/KC_u;->B:J

    sub-long v6, v2, v6

    add-long/2addr v4, v6

    iput-wide v4, p0, Lcom/kamcord/android/core/KC_u;->A:J

    iget v4, p0, Lcom/kamcord/android/core/KC_u;->z:I

    add-int/lit8 v4, v4, 0x1

    iput v4, p0, Lcom/kamcord/android/core/KC_u;->z:I
    :try_end_8
    .catch Ljava/lang/Throwable; {:try_start_8 .. :try_end_8} :catch_0

    goto/16 :goto_0

    :catch_0
    move-exception v2

    :try_start_9
    invoke-virtual {v2}, Ljava/lang/Throwable;->printStackTrace()V

    const-string v2, "Unexpected error in worker thread. Source: 9"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V
    :try_end_9
    .catch Ljava/lang/Throwable; {:try_start_9 .. :try_end_9} :catch_1

    goto/16 :goto_1

    :catch_1
    move-exception v1

    invoke-virtual {v1}, Ljava/lang/Throwable;->printStackTrace()V

    const-string v1, "Unexpected error in worker thread. Source: 7"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V

    goto :goto_5

    :catchall_0
    move-exception v1

    :try_start_a
    monitor-exit v2

    throw v1
    :try_end_a
    .catch Ljava/lang/Throwable; {:try_start_a .. :try_end_a} :catch_1

    :cond_c
    :try_start_b
    new-instance v2, Lcom/kamcord/android/core/KC_O;

    iget-object v6, p0, Lcom/kamcord/android/core/KC_u;->k:Lcom/kamcord/android/core/KC_V;

    invoke-direct {v2, v5, v6}, Lcom/kamcord/android/core/KC_O;-><init>(Lcom/kamcord/android/core/KC_x;Lcom/kamcord/android/core/KC_V;)V
    :try_end_b
    .catch Ljava/lang/Throwable; {:try_start_b .. :try_end_b} :catch_2
    .catchall {:try_start_b .. :try_end_b} :catchall_1

    goto/16 :goto_2

    :catch_2
    move-exception v2

    :try_start_c
    invoke-virtual {v2}, Ljava/lang/Throwable;->printStackTrace()V

    const-string v2, "Unexpected error in worker thread. Source: 8"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V
    :try_end_c
    .catchall {:try_start_c .. :try_end_c} :catchall_1

    goto/16 :goto_3

    :catchall_1
    move-exception v1

    :try_start_d
    monitor-exit v3

    throw v1
    :try_end_d
    .catch Ljava/lang/Throwable; {:try_start_d .. :try_end_d} :catch_1

    :cond_d
    :try_start_e
    sget-boolean v2, Lcom/kamcord/android/core/KC_u;->d:Z

    if-eqz v2, :cond_e

    new-instance v2, Lcom/kamcord/android/core/KC_K;

    invoke-direct {v2, v5}, Lcom/kamcord/android/core/KC_K;-><init>(Lcom/kamcord/android/core/KC_x;)V

    goto/16 :goto_2

    :cond_e
    new-instance v2, Lcom/kamcord/android/core/KC_L;

    invoke-direct {v2, v5}, Lcom/kamcord/android/core/KC_L;-><init>(Lcom/kamcord/android/core/KC_x;)V
    :try_end_e
    .catch Ljava/lang/Throwable; {:try_start_e .. :try_end_e} :catch_2
    .catchall {:try_start_e .. :try_end_e} :catchall_1

    goto/16 :goto_2

    :cond_f
    move v2, v0

    goto :goto_4
.end method

.method public final r()V
    .locals 6

    const/4 v5, 0x1

    :try_start_0
    invoke-static {}, Ljava/lang/System;->nanoTime()J

    move-result-wide v0

    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->i:Lcom/kamcord/android/core/KC_C;

    monitor-enter v2
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :try_start_1
    iget-object v3, p0, Lcom/kamcord/android/core/KC_u;->i:Lcom/kamcord/android/core/KC_C;

    iget v3, v3, Lcom/kamcord/android/core/KC_C;->a:I

    if-ne v3, v5, :cond_0

    iget-object v3, p0, Lcom/kamcord/android/core/KC_u;->i:Lcom/kamcord/android/core/KC_C;

    const/4 v4, 0x2

    iput v4, v3, Lcom/kamcord/android/core/KC_C;->a:I

    :cond_0
    monitor-exit v2
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    :try_start_2
    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->j:Lcom/kamcord/android/core/KC_B;

    monitor-enter v2
    :try_end_2
    .catch Ljava/lang/Throwable; {:try_start_2 .. :try_end_2} :catch_0

    :try_start_3
    iget-object v3, p0, Lcom/kamcord/android/core/KC_u;->j:Lcom/kamcord/android/core/KC_B;

    iget v3, v3, Lcom/kamcord/android/core/KC_B;->a:I

    if-ne v3, v5, :cond_1

    iget-object v3, p0, Lcom/kamcord/android/core/KC_u;->j:Lcom/kamcord/android/core/KC_B;

    const/4 v4, 0x0

    iput v4, v3, Lcom/kamcord/android/core/KC_B;->a:I

    iget-object v3, p0, Lcom/kamcord/android/core/KC_u;->j:Lcom/kamcord/android/core/KC_B;

    invoke-static {}, Lcom/kamcord/android/core/KC_s;->i()Landroid/graphics/Bitmap;

    move-result-object v4

    iput-object v4, v3, Lcom/kamcord/android/core/KC_B;->b:Landroid/graphics/Bitmap;

    iget-object v3, p0, Lcom/kamcord/android/core/KC_u;->j:Lcom/kamcord/android/core/KC_B;

    invoke-virtual {v3}, Ljava/lang/Object;->notify()V

    :cond_1
    monitor-exit v2
    :try_end_3
    .catchall {:try_start_3 .. :try_end_3} :catchall_1

    :try_start_4
    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->m:Lcom/kamcord/android/AudioSource;

    if-nez v2, :cond_2

    sget-object v2, Lcom/kamcord/android/core/KC_u;->n:Lcom/kamcord/android/FmodAudioSource;

    if-eqz v2, :cond_2

    sget-object v2, Lcom/kamcord/android/core/KC_u;->n:Lcom/kamcord/android/FmodAudioSource;

    invoke-virtual {p0, v2}, Lcom/kamcord/android/core/KC_u;->a(Lcom/kamcord/android/AudioSource;)V

    :cond_2
    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    if-eqz v2, :cond_3

    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v2, v0, v1}, Lcom/kamcord/android/core/KC_T;->a(J)V

    :cond_3
    iget-boolean v2, p0, Lcom/kamcord/android/core/KC_u;->E:Z

    if-eqz v2, :cond_4

    iget-boolean v2, p0, Lcom/kamcord/android/core/KC_u;->F:Z

    if-eqz v2, :cond_4

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_u;->u()Lcom/kamcord/android/core/KC_s;

    move-result-object v2

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_s;->j()V

    iget-object v2, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_u;->u()Lcom/kamcord/android/core/KC_s;

    move-result-object v3

    invoke-virtual {v2, v3, v0, v1}, Lcom/kamcord/android/core/KC_T;->a(Lcom/kamcord/android/core/KC_s;J)Z

    const/4 v0, 0x0

    invoke-static {v0}, Lcom/kamcord/android/core/KamcordNative;->setDefaultFramebuffer(I)V

    :cond_4
    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->k:Lcom/kamcord/android/core/KC_V;

    monitor-enter v1
    :try_end_4
    .catch Ljava/lang/Throwable; {:try_start_4 .. :try_end_4} :catch_0

    :try_start_5
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->k:Lcom/kamcord/android/core/KC_V;

    iget-boolean v0, v0, Lcom/kamcord/android/core/KC_V;->a:Z

    if-eqz v0, :cond_5

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->k:Lcom/kamcord/android/core/KC_V;

    const/4 v2, 0x0

    iput-boolean v2, v0, Lcom/kamcord/android/core/KC_V;->a:Z

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->k:Lcom/kamcord/android/core/KC_V;

    invoke-virtual {v0}, Ljava/lang/Object;->notify()V

    :cond_5
    monitor-exit v1
    :try_end_5
    .catchall {:try_start_5 .. :try_end_5} :catchall_2

    :try_start_6
    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->h:Lcom/kamcord/android/core/KC_A;

    monitor-enter v1
    :try_end_6
    .catch Ljava/lang/Throwable; {:try_start_6 .. :try_end_6} :catch_0

    :try_start_7
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->h:Lcom/kamcord/android/core/KC_A;

    iget-boolean v0, v0, Lcom/kamcord/android/core/KC_A;->a:Z

    if-eqz v0, :cond_6

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->h:Lcom/kamcord/android/core/KC_A;

    const/4 v2, 0x0

    iput-boolean v2, v0, Lcom/kamcord/android/core/KC_A;->a:Z

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->h:Lcom/kamcord/android/core/KC_A;

    invoke-virtual {v0}, Ljava/lang/Object;->notify()V

    :cond_6
    monitor-exit v1
    :try_end_7
    .catchall {:try_start_7 .. :try_end_7} :catchall_3

    :try_start_8
    iget-object v1, p0, Lcom/kamcord/android/core/KC_u;->g:Lcom/kamcord/android/core/KC_E;

    monitor-enter v1
    :try_end_8
    .catch Ljava/lang/Throwable; {:try_start_8 .. :try_end_8} :catch_0

    :try_start_9
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->g:Lcom/kamcord/android/core/KC_E;

    iget-boolean v0, v0, Lcom/kamcord/android/core/KC_E;->a:Z

    if-eqz v0, :cond_9

    const/4 v0, 0x0

    invoke-static {v0}, Lcom/kamcord/android/core/KamcordNative;->setDefaultFramebuffer(I)V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->q:Lcom/kamcord/android/core/KC_T;

    iget-object v2, v0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    if-eqz v2, :cond_7

    iget-object v2, v0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_I;->f()V

    :cond_7
    iget-object v2, v0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    if-eqz v2, :cond_8

    iget-object v2, v0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    :cond_8
    const/4 v2, 0x0

    iput-object v2, v0, Lcom/kamcord/android/core/KC_T;->a:Lcom/kamcord/android/core/KC_I;

    const/4 v2, 0x0

    iput-object v2, v0, Lcom/kamcord/android/core/KC_T;->b:Lcom/kamcord/android/core/KC_a;

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->g:Lcom/kamcord/android/core/KC_E;

    const/4 v2, 0x0

    iput-boolean v2, v0, Lcom/kamcord/android/core/KC_E;->a:Z

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->g:Lcom/kamcord/android/core/KC_E;

    invoke-virtual {v0}, Ljava/lang/Object;->notify()V

    :cond_9
    monitor-exit v1
    :try_end_9
    .catchall {:try_start_9 .. :try_end_9} :catchall_4

    :goto_0
    return-void

    :catchall_0
    move-exception v0

    :try_start_a
    monitor-exit v2

    throw v0
    :try_end_a
    .catch Ljava/lang/Throwable; {:try_start_a .. :try_end_a} :catch_0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    const-string v0, "Unexpected error in worker thread. Source: 10"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V

    goto :goto_0

    :catchall_1
    move-exception v0

    :try_start_b
    monitor-exit v2

    throw v0

    :catchall_2
    move-exception v0

    monitor-exit v1

    throw v0

    :catchall_3
    move-exception v0

    monitor-exit v1

    throw v0

    :catchall_4
    move-exception v0

    monitor-exit v1

    throw v0
    :try_end_b
    .catch Ljava/lang/Throwable; {:try_start_b .. :try_end_b} :catch_0
.end method

.method public final s()V
    .locals 1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->D:Lcom/kamcord/android/core/KC_s;

    if-eqz v0, :cond_0

    const-string v0, "Deleting framebuffers."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_u;->D:Lcom/kamcord/android/core/KC_s;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_s;->g()V

    :goto_0
    return-void

    :cond_0
    const-string v0, "Attempt to delete framebuffers with no openGL object."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    goto :goto_0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    const-string v0, "Unexpected error in worker thread. Source: 12"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V

    goto :goto_0
.end method
