.class final Lcom/kamcord/a/a/f/KC_f;
.super Ljava/lang/Object;


# instance fields
.field private final a:Ljava/util/Random;


# direct methods
.method constructor <init>()V
    .locals 1

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    new-instance v0, Ljava/util/Random;

    invoke-direct {v0}, Ljava/util/Random;-><init>()V

    iput-object v0, p0, Lcom/kamcord/a/a/f/KC_f;->a:Ljava/util/Random;

    return-void
.end method


# virtual methods
.method final a()Ljava/lang/Integer;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/a/a/f/KC_f;->a:Ljava/util/Random;

    invoke-virtual {v0}, Ljava/util/Random;->nextInt()I

    move-result v0

    invoke-static {v0}, Ljava/lang/Integer;->valueOf(I)Ljava/lang/Integer;

    move-result-object v0

    return-object v0
.end method
