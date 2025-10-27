.class final Lcom/kamcord/android/KC_Y$KC_b;
.super Ljava/io/OutputStream;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_Y;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = "KC_b"
.end annotation


# instance fields
.field private final a:Ljava/io/OutputStream;

.field private synthetic b:Lcom/kamcord/android/KC_Y;


# direct methods
.method public constructor <init>(Lcom/kamcord/android/KC_Y;Ljava/io/OutputStream;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_Y$KC_b;->b:Lcom/kamcord/android/KC_Y;

    invoke-direct {p0}, Ljava/io/OutputStream;-><init>()V

    iput-object p2, p0, Lcom/kamcord/android/KC_Y$KC_b;->a:Ljava/io/OutputStream;

    return-void
.end method

.method private a(J)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_Y$KC_b;->b:Lcom/kamcord/android/KC_Y;

    invoke-static {v0, p1, p2}, Lcom/kamcord/android/KC_Y;->a(Lcom/kamcord/android/KC_Y;J)J

    iget-object v0, p0, Lcom/kamcord/android/KC_Y$KC_b;->b:Lcom/kamcord/android/KC_Y;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Y;->a()V

    return-void
.end method


# virtual methods
.method public final close()V
    .locals 1
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/io/IOException;
        }
    .end annotation

    iget-object v0, p0, Lcom/kamcord/android/KC_Y$KC_b;->a:Ljava/io/OutputStream;

    invoke-virtual {v0}, Ljava/io/OutputStream;->close()V

    return-void
.end method

.method public final flush()V
    .locals 1
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/io/IOException;
        }
    .end annotation

    iget-object v0, p0, Lcom/kamcord/android/KC_Y$KC_b;->a:Ljava/io/OutputStream;

    invoke-virtual {v0}, Ljava/io/OutputStream;->flush()V

    return-void
.end method

.method public final write(I)V
    .locals 2
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/io/IOException;
        }
    .end annotation

    iget-object v0, p0, Lcom/kamcord/android/KC_Y$KC_b;->a:Ljava/io/OutputStream;

    invoke-virtual {v0, p1}, Ljava/io/OutputStream;->write(I)V

    const-wide/16 v0, 0x1

    invoke-direct {p0, v0, v1}, Lcom/kamcord/android/KC_Y$KC_b;->a(J)V

    return-void
.end method

.method public final write([B)V
    .locals 2
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/io/IOException;
        }
    .end annotation

    iget-object v0, p0, Lcom/kamcord/android/KC_Y$KC_b;->a:Ljava/io/OutputStream;

    invoke-virtual {v0, p1}, Ljava/io/OutputStream;->write([B)V

    array-length v0, p1

    int-to-long v0, v0

    invoke-direct {p0, v0, v1}, Lcom/kamcord/android/KC_Y$KC_b;->a(J)V

    return-void
.end method

.method public final write([BII)V
    .locals 2
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/io/IOException;
        }
    .end annotation

    iget-object v0, p0, Lcom/kamcord/android/KC_Y$KC_b;->a:Ljava/io/OutputStream;

    invoke-virtual {v0, p1, p2, p3}, Ljava/io/OutputStream;->write([BII)V

    int-to-long v0, p3

    invoke-direct {p0, v0, v1}, Lcom/kamcord/android/KC_Y$KC_b;->a(J)V

    return-void
.end method
